using System;
using System.Collections;
using System.Collections.Generic;
using DimensionBrawl.Combat;
using UnityEngine;
using UnityEngine.UI;

namespace DimensionBrawl.Presentation
{
    [DisallowMultipleComponent]
    public sealed class CinematicSequenceRunner : MonoBehaviour
    {
        [Serializable]
        public struct ActorBinding
        {
            [SerializeField] private CinematicSequenceProfile.ActorRole role;
            [SerializeField] private Animator bodyAnimator;
            [SerializeField] private Animator faceAnimator;
            [SerializeField] private CinematicBlendShapeExpressionPlayer expressionPlayer;
            [SerializeField] private Transform anchor;

            public ActorBinding(
                CinematicSequenceProfile.ActorRole role,
                Animator bodyAnimator,
                Animator faceAnimator,
                CinematicBlendShapeExpressionPlayer expressionPlayer,
                Transform anchor)
            {
                this.role = role;
                this.bodyAnimator = bodyAnimator;
                this.faceAnimator = faceAnimator;
                this.expressionPlayer = expressionPlayer;
                this.anchor = anchor;
            }

            public CinematicSequenceProfile.ActorRole Role => role;
            public Animator BodyAnimator => bodyAnimator;
            public Animator FaceAnimator => faceAnimator != null ? faceAnimator : bodyAnimator;
            public CinematicBlendShapeExpressionPlayer ExpressionPlayer => expressionPlayer;
            public Transform Anchor => anchor != null ? anchor : (bodyAnimator != null ? bodyAnimator.transform : null);
        }

        [Serializable]
        private struct SourceCameraBinding
        {
            [SerializeField] private string cueId;
            [SerializeField] private GameObject rigRoot;
            [SerializeField] private Transform cameraTransform;
            [SerializeField] private Camera cameraComponent;

            public SourceCameraBinding(
                string cueId,
                GameObject rigRoot,
                Transform cameraTransform,
                Camera cameraComponent)
            {
                this.cueId = cueId;
                this.rigRoot = rigRoot;
                this.cameraTransform = cameraTransform;
                this.cameraComponent = cameraComponent;
            }

            public string CueId => cueId;
            public GameObject RigRoot => rigRoot;
            public Transform CameraTransform => cameraTransform != null
                ? cameraTransform
                : (cameraComponent != null ? cameraComponent.transform : null);
            public Camera CameraComponent => cameraComponent;
            public bool IsValid => rigRoot != null && CameraTransform != null;

            public bool Matches(string candidateCueId) =>
                string.Equals(cueId, candidateCueId, StringComparison.Ordinal);
        }

        [Serializable]
        private struct SourceActorBinding
        {
            [SerializeField] private string cueId;
            [SerializeField] private GameObject rigRoot;
            [SerializeField] private GameObject visibilityRoot;

            public SourceActorBinding(string cueId, GameObject rigRoot, GameObject visibilityRoot)
            {
                this.cueId = cueId;
                this.rigRoot = rigRoot;
                this.visibilityRoot = visibilityRoot;
            }

            public string CueId => cueId;
            public GameObject RigRoot => rigRoot;
            public GameObject VisibilityRoot => visibilityRoot != null ? visibilityRoot : rigRoot;
            public bool IsValid => rigRoot != null;

            public bool Matches(string candidateCueId) =>
                string.Equals(cueId, candidateCueId, StringComparison.Ordinal);
        }

        [Header("Profile")]
        [SerializeField] private CinematicSequenceProfile sequenceProfile;

        [Header("Bindings")]
        [SerializeField] private ActorBinding[] actorBindings = Array.Empty<ActorBinding>();
        [SerializeField] private RuntimeAnimatorController bodyControllerOverride;
        [SerializeField] private ActionCameraController cameraController;
        [SerializeField] private CombatVfxCuePlayer combatVfxCuePlayer;
        [SerializeField] private CinematicTutorialPromptPresenter tutorialPromptPresenter;
        [SerializeField] private Transform cueSpace;

        [Header("Shot Camera")]
        [SerializeField] private Camera cinematicCamera;
        [SerializeField] private bool driveCameraTransformFromProfile = true;
        [SerializeField] private bool disableActionCameraControllerDuringPoseDrive = true;
        [SerializeField] private GameObject sourceCameraRigRoot;
        [SerializeField] private Transform sourceCameraTransform;
        [SerializeField] private Camera sourceCameraComponent;
        [SerializeField] private SourceCameraBinding[] sourceCameraBindings =
            Array.Empty<SourceCameraBinding>();

        [Header("Source Actor")]
        [SerializeField] private GameObject sourceActorRigRoot;
        [SerializeField] private GameObject sourceActorVisibilityRoot;
        [SerializeField] private SourceActorBinding[] sourceActorBindings =
            Array.Empty<SourceActorBinding>();
        [SerializeField] private Transform[] sourceActorGradeExcludedRoots =
            Array.Empty<Transform>();
        [SerializeField] private GameObject primaryActorRootHiddenDuringSourceActorAnimation;

        [Header("Screen Fade")]
        [SerializeField] private CanvasGroup screenFadeCanvasGroup;
        [SerializeField] private Image screenFadeImage;

        [Header("Playback")]
        [SerializeField, Min(0.001f)] private float maxPlaybackDeltaSeconds = 1f / 30f;

        [Header("Action Cue Bridge")]
        [SerializeField] private bool playLinkedActionCueOnStart;
        [SerializeField] private ActionCinematicCueDirector linkedActionCueDirector;
        [SerializeField] private ActionCinematicCueProfile.CueKind linkedActionCueKind =
            ActionCinematicCueProfile.CueKind.SkillCutIn;
        [SerializeField, Min(1)] private int linkedActionCueTier = 1;

        [Header("Playback Locks")]
        [SerializeField] private Behaviour[] behavioursDisabledDuringPlayback = Array.Empty<Behaviour>();

        private Coroutine activeRoutine;
        private int totalCameraCueCount;
        private int totalActorCueCount;
        private int totalBoundActorCueCount;
        private int totalVfxCueCount;
        private int totalTutorialCueCount;
        private string lastCameraCueId;
        private string lastActorCueId;
        private string lastVfxCueId;
        private string lastTutorialCueId;
        private bool gameplayHandoffReached;
        private Coroutine activeCameraPoseRoutine;
        private bool cameraStateCaptured;
        private Vector3 originalCameraPosition;
        private Quaternion originalCameraRotation;
        private float originalCameraFieldOfView;
        private bool originalCameraControllerEnabled;
        private bool cameraControllerStateCaptured;
        private RuntimeAnimatorController[] originalBodyControllers = Array.Empty<RuntimeAnimatorController>();
        private bool bodyControllerStateCaptured;
        private bool forceImmediateCameraPoseForReviewSample;
        private bool[] originalPlaybackLockEnabledStates = Array.Empty<bool>();
        private bool playbackLockStateCaptured;
        private bool sourceCameraCueApplied;
        private bool sourceActorStateCaptured;
        private bool originalSourceActorActive;
        private bool[] originalSourceActorBindingActiveStates = Array.Empty<bool>();
        private bool originalPrimaryActorActive;
        private bool sourceActorCueApplied;
        private bool sourceActorGradeApplied;
        private MaterialPropertyBlock sourceActorGradeBlock;
        private readonly HashSet<string> activeSourceActorCueIds = new HashSet<string>(StringComparer.Ordinal);
        private static readonly int SourceActorGradeBaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int SourceActorGradeColorId = Shader.PropertyToID("_Color");
        private static readonly int SourceActorGradeEmissionColorId = Shader.PropertyToID("_EmissionColor");
        private static readonly int SourceActorGradeFirstShadeColorId = Shader.PropertyToID("_1st_ShadeColor");
        private static readonly int SourceActorGradeSecondShadeColorId = Shader.PropertyToID("_2nd_ShadeColor");

        public CinematicSequenceProfile SequenceProfile => sequenceProfile;
        public Camera CinematicCamera => cinematicCamera != null ? cinematicCamera : ResolveCinematicCamera();
        public bool DriveCameraTransformFromProfile => driveCameraTransformFromProfile;
        public bool IsPlaying => activeRoutine != null;
        public int TotalCameraCueCount => totalCameraCueCount;
        public int TotalActorCueCount => totalActorCueCount;
        public int TotalBoundActorCueCount => totalBoundActorCueCount;
        public int TotalVfxCueCount => totalVfxCueCount;
        public int TotalTutorialCueCount => totalTutorialCueCount;
        public CinematicTutorialPromptPresenter TutorialPromptPresenter => tutorialPromptPresenter;
        public string LastCameraCueId => lastCameraCueId;
        public string LastActorCueId => lastActorCueId;
        public string LastVfxCueId => lastVfxCueId;
        public string LastTutorialCueId => lastTutorialCueId;
        public bool GameplayHandoffReached => gameplayHandoffReached;

        private void Awake()
        {
            if (cueSpace == null)
            {
                cueSpace = transform;
            }

            if (cameraController == null)
            {
                cameraController = GetComponent<ActionCameraController>();
            }

            if (cinematicCamera == null)
            {
                cinematicCamera = ResolveCinematicCamera();
            }

            if (combatVfxCuePlayer == null)
            {
                combatVfxCuePlayer = GetComponent<CombatVfxCuePlayer>();
            }

            if (tutorialPromptPresenter == null)
            {
                tutorialPromptPresenter = GetComponent<CinematicTutorialPromptPresenter>();
            }

            if (linkedActionCueDirector == null)
            {
                linkedActionCueDirector = GetComponent<ActionCinematicCueDirector>();
            }
        }

        private void OnDisable()
        {
            Stop();
        }

        public bool TryPlay()
        {
            return TryPlay(ResolveDefaultDirection());
        }

        public bool TryPlayProfile(CinematicSequenceProfile profile)
        {
            return TryPlayProfile(profile, ResolveDefaultDirection());
        }

        public bool TryPlayProfile(CinematicSequenceProfile profile, Vector3 planarDirection)
        {
            if (!isActiveAndEnabled || profile == null || activeRoutine != null)
            {
                return false;
            }

            sequenceProfile = profile;
            return TryPlay(planarDirection);
        }

        public bool TryApplyProfileSampleForReview(
            CinematicSequenceProfile profile,
            float elapsedSeconds,
            Vector3 planarDirection)
        {
            if (profile == null || activeRoutine != null)
            {
                return false;
            }

            sequenceProfile = profile;
            ResetCounters();

            bool[] cameraPlayed = new bool[sequenceProfile.CameraCues.Length];
            bool[] actorPlayed = new bool[sequenceProfile.ActorCues.Length];
            bool[] vfxPlayed = new bool[sequenceProfile.VfxCues.Length];
            bool[] tutorialPlayed = new bool[sequenceProfile.TutorialCues.Length];
            bool handoffPlayed = false;
            forceImmediateCameraPoseForReviewSample = true;

            try
            {
                DispatchDueCues(
                    Mathf.Max(0f, elapsedSeconds),
                    planarDirection,
                    cameraPlayed,
                    actorPlayed,
                    vfxPlayed,
                    tutorialPlayed,
                    ref handoffPlayed);
                SampleSourceCameraAnimation(Mathf.Max(0f, elapsedSeconds));
                SampleSourceActorAnimation(Mathf.Max(0f, elapsedSeconds));
                SampleSourceActorGrade(Mathf.Max(0f, elapsedSeconds));
                SampleScreenFade(Mathf.Max(0f, elapsedSeconds));
                SampleActorStatesForReview(Mathf.Max(0f, elapsedSeconds));
            }
            finally
            {
                forceImmediateCameraPoseForReviewSample = false;
            }

            return true;
        }

        private void SampleActorStatesForReview(float elapsedSeconds)
        {
            if (sequenceProfile == null || actorBindings == null)
            {
                return;
            }

            for (int i = 0; i < actorBindings.Length; i++)
            {
                ActorBinding binding = actorBindings[i];
                Animator animator = binding.BodyAnimator;
                if (animator == null || !animator.isActiveAndEnabled)
                {
                    continue;
                }

                CinematicSequenceProfile.ActorCue? selectedCue = null;
                float selectedStartSeconds = float.NegativeInfinity;
                CinematicSequenceProfile.ActorCue[] actorCues = sequenceProfile.ActorCues;
                for (int j = 0; j < actorCues.Length; j++)
                {
                    CinematicSequenceProfile.ActorCue cue = actorCues[j];
                    if (!cue.Enabled
                        || cue.Role != binding.Role
                        || cue.CueKind != CinematicSequenceProfile.ActorCueKind.BodyState
                        || cue.StartSeconds > elapsedSeconds
                        || string.IsNullOrWhiteSpace(cue.AnimatorStateName))
                    {
                        continue;
                    }

                    if (cue.StartSeconds >= selectedStartSeconds)
                    {
                        selectedCue = cue;
                        selectedStartSeconds = cue.StartSeconds;
                    }
                }

                if (!selectedCue.HasValue)
                {
                    continue;
                }

                CinematicSequenceProfile.ActorCue bodyCue = selectedCue.Value;
                float localSeconds = Mathf.Max(0f, elapsedSeconds - bodyCue.StartSeconds);
                float normalizedTime = ResolveStateNormalizedTime(animator, bodyCue.AnimatorStateName, localSeconds);
                animator.Play(bodyCue.AnimatorStateName, 0, normalizedTime);
                animator.Update(0.01f);
            }
        }

        private static float ResolveStateNormalizedTime(Animator animator, string stateName, float localSeconds)
        {
            if (animator == null || animator.runtimeAnimatorController == null)
            {
                return 0f;
            }

            AnimationClip clip = null;
            AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
            for (int i = 0; i < clips.Length; i++)
            {
                if (clips[i] != null && string.Equals(clips[i].name, stateName, StringComparison.Ordinal))
                {
                    clip = clips[i];
                    break;
                }
            }

            if (clip == null || clip.length <= 0.01f)
            {
                return 0f;
            }

            float normalized = localSeconds / clip.length;
            return clip.isLooping ? Mathf.Repeat(normalized, 1f) : Mathf.Clamp01(normalized);
        }

        public bool TryPlay(Vector3 planarDirection)
        {
            if (!isActiveAndEnabled || sequenceProfile == null || activeRoutine != null)
            {
                return false;
            }

            ResetCounters();
            PrepareCameraForSequence();
            PrepareSourceActorAnimationForSequence();
            DisablePlaybackLockedBehaviours();
            activeRoutine = StartCoroutine(PlayRoutine(planarDirection));
            return true;
        }

        public void Stop()
        {
            if (activeRoutine != null)
            {
                StopCoroutine(activeRoutine);
            }

            if (activeCameraPoseRoutine != null)
            {
                StopCoroutine(activeCameraPoseRoutine);
            }

            activeRoutine = null;
            activeCameraPoseRoutine = null;
            if (tutorialPromptPresenter != null)
            {
                tutorialPromptPresenter.HidePrompt();
            }

            RestoreCameraStateIfNeeded();
            RestoreBodyControllersIfNeeded();
            ClearSourceActorGrade();
            ClearScreenFade();
            RestoreSourceActorStateIfNeeded();
            RestorePlaybackLockedBehavioursIfNeeded();
        }

        private IEnumerator PlayRoutine(Vector3 planarDirection)
        {
            PrepareBodyControllersForSequence();
            bool[] cameraPlayed = new bool[sequenceProfile.CameraCues.Length];
            bool[] actorPlayed = new bool[sequenceProfile.ActorCues.Length];
            bool[] vfxPlayed = new bool[sequenceProfile.VfxCues.Length];
            bool[] tutorialPlayed = new bool[sequenceProfile.TutorialCues.Length];
            bool handoffPlayed = false;
            float elapsed = 0f;
            float duration = sequenceProfile.EstimatedDurationSeconds;

            if (playLinkedActionCueOnStart && linkedActionCueDirector != null)
            {
                linkedActionCueDirector.TryPlay(linkedActionCueKind, linkedActionCueTier, planarDirection);
            }

            DispatchDueCues(
                elapsed,
                planarDirection,
                cameraPlayed,
                actorPlayed,
                vfxPlayed,
                tutorialPlayed,
                ref handoffPlayed);
            SampleSourceCameraAnimation(elapsed);
            SampleSourceActorAnimation(elapsed);
            SampleSourceActorGrade(elapsed);
            SampleScreenFade(elapsed);

            while (elapsed < duration)
            {
                float deltaTime = ResolvePlaybackDeltaSeconds();
                elapsed += Mathf.Max(0f, deltaTime);
                DispatchDueCues(
                    elapsed,
                    planarDirection,
                    cameraPlayed,
                    actorPlayed,
                    vfxPlayed,
                    tutorialPlayed,
                    ref handoffPlayed);
                SampleSourceCameraAnimation(elapsed);
                SampleSourceActorAnimation(elapsed);
                SampleSourceActorGrade(elapsed);
                SampleScreenFade(elapsed);
                yield return null;
            }

            activeRoutine = null;
            if (activeCameraPoseRoutine != null)
            {
                StopCoroutine(activeCameraPoseRoutine);
            }

            activeCameraPoseRoutine = null;
            RestoreCameraStateIfNeeded();
            RestoreBodyControllersIfNeeded();
            ClearSourceActorGrade();
            ClearScreenFade();
            RestoreSourceActorStateIfNeeded();
            RestorePlaybackLockedBehavioursIfNeeded();
        }

        private float ResolvePlaybackDeltaSeconds()
        {
            float rawDelta = sequenceProfile != null && sequenceProfile.UseUnscaledClock
                ? Time.unscaledDeltaTime
                : Time.deltaTime;
            if (float.IsNaN(rawDelta) || float.IsInfinity(rawDelta))
            {
                return 0f;
            }

            return Mathf.Min(Mathf.Max(0f, rawDelta), Mathf.Max(0.001f, maxPlaybackDeltaSeconds));
        }

        private void DispatchDueCues(
            float elapsed,
            Vector3 planarDirection,
            bool[] cameraPlayed,
            bool[] actorPlayed,
            bool[] vfxPlayed,
            bool[] tutorialPlayed,
            ref bool handoffPlayed)
        {
            CinematicSequenceProfile.CameraCue[] cameraCues = sequenceProfile.CameraCues;
            for (int i = 0; i < cameraCues.Length; i++)
            {
                if (!cameraPlayed[i] && cameraCues[i].Enabled && elapsed >= cameraCues[i].StartSeconds)
                {
                    cameraPlayed[i] = true;
                    DispatchCameraCue(cameraCues[i], planarDirection);
                }
            }

            CinematicSequenceProfile.ActorCue[] actorCues = sequenceProfile.ActorCues;
            for (int i = 0; i < actorCues.Length; i++)
            {
                if (!actorPlayed[i] && actorCues[i].Enabled && elapsed >= actorCues[i].StartSeconds)
                {
                    actorPlayed[i] = true;
                    DispatchActorCue(actorCues[i]);
                }
            }

            CinematicSequenceProfile.VfxCue[] vfxCues = sequenceProfile.VfxCues;
            for (int i = 0; i < vfxCues.Length; i++)
            {
                if (!vfxPlayed[i] && vfxCues[i].Enabled && elapsed >= vfxCues[i].StartSeconds)
                {
                    vfxPlayed[i] = true;
                    DispatchVfxCue(vfxCues[i], planarDirection);
                }
            }

            CinematicSequenceProfile.TutorialCue[] tutorialCues = sequenceProfile.TutorialCues;
            for (int i = 0; i < tutorialCues.Length; i++)
            {
                if (!tutorialPlayed[i] && tutorialCues[i].Enabled && elapsed >= tutorialCues[i].StartSeconds)
                {
                    tutorialPlayed[i] = true;
                    DispatchTutorialCue(tutorialCues[i]);
                }
            }

            CinematicSequenceProfile.GameplayHandoffCue handoff = sequenceProfile.GameplayHandoff;
            if (!handoffPlayed && handoff.Enabled && elapsed >= handoff.StartSeconds)
            {
                handoffPlayed = true;
                gameplayHandoffReached = true;
            }
        }

        private void DispatchCameraCue(CinematicSequenceProfile.CameraCue cue, Vector3 planarDirection)
        {
            totalCameraCueCount++;
            lastCameraCueId = cue.CueId;
            if (driveCameraTransformFromProfile && cue.DriveCameraPose && TryDriveCameraPose(cue))
            {
                return;
            }

            if (cameraController == null)
            {
                return;
            }

            Transform space = cueSpace != null ? cueSpace : transform;
            Vector3 offset = space.TransformDirection(cue.LocalOffset);
            Vector3 direction = Vector3.ProjectOnPlane(planarDirection, Vector3.up);
            if (direction.sqrMagnitude > 0.0001f)
            {
                offset += direction.normalized * cue.PlanarDirectionOffset;
            }

            float scale = cue.ImpulseScale;
            cameraController.RequestCue(
                offset * scale,
                cue.DurationSeconds,
                cue.FieldOfViewDelta * scale,
                cue.CameraDistanceDelta * scale,
                cue.FocusHeightDelta * scale);
        }

        private void DispatchActorCue(CinematicSequenceProfile.ActorCue cue)
        {
            totalActorCueCount++;
            lastActorCueId = cue.CueId;
            if (!TryFindBinding(cue.Role, out ActorBinding binding))
            {
                return;
            }

            Animator animator = RequiresFaceAnimator(cue.CueKind) ? binding.FaceAnimator : binding.BodyAnimator;
            if (animator == null)
            {
                return;
            }

            if (cue.ControllerOverride != null)
            {
                animator.runtimeAnimatorController = cue.ControllerOverride;
            }

            totalBoundActorCueCount++;
            switch (cue.CueKind)
            {
                case CinematicSequenceProfile.ActorCueKind.BodyState:
                    CrossFadeState(animator, cue.AnimatorStateName, cue.DurationSeconds);
                    break;
                case CinematicSequenceProfile.ActorCueKind.BodyTrigger:
                    TriggerAnimator(animator, cue.AnimatorTriggerName);
                    break;
                case CinematicSequenceProfile.ActorCueKind.FaceState:
                    if (binding.ExpressionPlayer == null || !binding.ExpressionPlayer.PlayExpression(ResolveFaceStateName(cue)))
                    {
                        CrossFadeState(animator, ResolveFaceStateName(cue), cue.DurationSeconds);
                    }

                    break;
                case CinematicSequenceProfile.ActorCueKind.FaceTrigger:
                    TriggerAnimator(animator, cue.AnimatorTriggerName);
                    break;
                case CinematicSequenceProfile.ActorCueKind.WeaponAttach:
                    ValidateSocket(binding.Anchor, cue.SocketPath, cue.RequireSocket);
                    break;
                case CinematicSequenceProfile.ActorCueKind.WeaponVisibility:
                    SetActorObjectActive(binding.Anchor, cue.SocketPath, cue.RequireSocket, cue.ObjectActive);
                    break;
            }
        }

        private void DispatchVfxCue(CinematicSequenceProfile.VfxCue cue, Vector3 planarDirection)
        {
            totalVfxCueCount++;
            lastVfxCueId = cue.CueId;
            if (cue.UseCombatVfxCue && combatVfxCuePlayer != null)
            {
                Transform anchor = cueSpace != null ? cueSpace : transform;
                combatVfxCuePlayer.PlayCue(
                    cue.CombatVfxCueId,
                    anchor,
                    planarDirection,
                    cue.Intensity,
                    -1f,
                    cue.LocalOffset);
            }
        }

        private void DispatchTutorialCue(CinematicSequenceProfile.TutorialCue cue)
        {
            totalTutorialCueCount++;
            lastTutorialCueId = cue.CueId;
            if (tutorialPromptPresenter != null)
            {
                tutorialPromptPresenter.ShowCue(cue);
            }
        }

        private bool TryFindBinding(CinematicSequenceProfile.ActorRole role, out ActorBinding binding)
        {
            for (int i = 0; i < actorBindings.Length; i++)
            {
                if (actorBindings[i].Role == role)
                {
                    binding = actorBindings[i];
                    return true;
                }
            }

            if (role == CinematicSequenceProfile.ActorRole.Summon
                && TryResolveActiveSummonBinding(out binding))
            {
                return true;
            }

            binding = default;
            return false;
        }

        private static bool TryResolveActiveSummonBinding(out ActorBinding binding)
        {
            for (int i = SummonFrontlineProxy.ActiveRegisteredProxyCount - 1; i >= 0; i--)
            {
                if (!SummonFrontlineProxy.TryGetActiveRegisteredProxy(i, out SummonFrontlineProxy proxy)
                    || proxy == null
                    || !proxy.IsPresentationVisible)
                {
                    continue;
                }

                Animator animator = proxy.GetComponentInChildren<Animator>(includeInactive: true);
                if (animator == null)
                {
                    continue;
                }

                binding = new ActorBinding(
                    CinematicSequenceProfile.ActorRole.Summon,
                    animator,
                    null,
                    null,
                    proxy.transform);
                return true;
            }

            binding = default;
            return false;
        }

        private static bool RequiresFaceAnimator(CinematicSequenceProfile.ActorCueKind cueKind)
        {
            return cueKind == CinematicSequenceProfile.ActorCueKind.FaceState
                || cueKind == CinematicSequenceProfile.ActorCueKind.FaceTrigger;
        }

        private static void CrossFadeState(Animator animator, string stateName, float durationSeconds)
        {
            if (animator == null || !animator.isActiveAndEnabled || string.IsNullOrWhiteSpace(stateName))
            {
                return;
            }

            float transitionSeconds = Mathf.Clamp(durationSeconds * 0.12f, 0.04f, 0.16f);
            animator.CrossFadeInFixedTime(stateName, transitionSeconds);
        }

        private static void TriggerAnimator(Animator animator, string triggerName)
        {
            if (animator == null
                || !animator.isActiveAndEnabled
                || string.IsNullOrWhiteSpace(triggerName)
                || !HasAnimatorTrigger(animator, triggerName))
            {
                return;
            }

            animator.SetTrigger(triggerName);
        }

        private static bool HasAnimatorTrigger(Animator animator, string triggerName)
        {
            if (animator.runtimeAnimatorController == null)
            {
                return false;
            }

            AnimatorControllerParameter[] parameters = animator.parameters;
            for (int i = 0; i < parameters.Length; i++)
            {
                AnimatorControllerParameter parameter = parameters[i];
                if (parameter.type == AnimatorControllerParameterType.Trigger
                    && string.Equals(parameter.name, triggerName, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private static string ResolveFaceStateName(CinematicSequenceProfile.ActorCue cue)
        {
            return !string.IsNullOrWhiteSpace(cue.FaceStateName)
                ? cue.FaceStateName
                : cue.AnimatorStateName;
        }

        private static bool ValidateSocket(Transform root, string socketPath, bool required)
        {
            if (!required)
            {
                return true;
            }

            if (root == null || string.IsNullOrWhiteSpace(socketPath))
            {
                return false;
            }

            return root.Find(socketPath) != null;
        }

        private static bool SetActorObjectActive(Transform root, string objectPath, bool required, bool active)
        {
            Transform target = ResolveActorObject(root, objectPath);
            if (target == null)
            {
                return !required;
            }

            target.gameObject.SetActive(active);
            return true;
        }

        private static Transform ResolveActorObject(Transform root, string objectPath)
        {
            if (root == null)
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(objectPath))
            {
                return root;
            }

            Transform found = root.Find(objectPath);
            if (found != null)
            {
                return found;
            }

            return FindDescendantByName(root, objectPath) ?? FindDescendantContains(root, objectPath);
        }

        private static Transform FindDescendantByName(Transform root, string objectName)
        {
            if (string.Equals(root.name, objectName, StringComparison.OrdinalIgnoreCase))
            {
                return root;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                Transform found = FindDescendantByName(root.GetChild(i), objectName);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private static Transform FindDescendantContains(Transform root, string objectNamePart)
        {
            if (root.name.IndexOf(objectNamePart, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return root;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                Transform found = FindDescendantContains(root.GetChild(i), objectNamePart);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private void PrepareCameraForSequence()
        {
            cameraStateCaptured = false;
            cameraControllerStateCaptured = false;
            sourceCameraCueApplied = false;
            if (!driveCameraTransformFromProfile || sequenceProfile == null || !SequenceUsesDrivenCameraPose())
            {
                return;
            }

            Camera camera = ResolveCinematicCamera();
            if (camera == null)
            {
                return;
            }

            originalCameraPosition = camera.transform.position;
            originalCameraRotation = camera.transform.rotation;
            originalCameraFieldOfView = camera.fieldOfView;
            cameraStateCaptured = true;

            if (cameraController != null)
            {
                originalCameraControllerEnabled = cameraController.enabled;
                cameraControllerStateCaptured = true;
                if (disableActionCameraControllerDuringPoseDrive)
                {
                    cameraController.enabled = false;
                }
            }

            if (!PrewarmSourceCameraAnimation(camera))
            {
                PrewarmOpeningCameraPose(camera);
            }
        }

        private void PrepareBodyControllersForSequence()
        {
            bodyControllerStateCaptured = false;
            if (bodyControllerOverride == null || actorBindings == null || actorBindings.Length == 0)
            {
                return;
            }

            originalBodyControllers = new RuntimeAnimatorController[actorBindings.Length];
            for (int i = 0; i < actorBindings.Length; i++)
            {
                Animator animator = actorBindings[i].BodyAnimator;
                if (animator == null)
                {
                    continue;
                }

                originalBodyControllers[i] = animator.runtimeAnimatorController;
                if (ShouldApplyBodyControllerOverride(actorBindings[i].Role)
                    && animator.runtimeAnimatorController != bodyControllerOverride)
                {
                    animator.runtimeAnimatorController = bodyControllerOverride;
                }
            }

            bodyControllerStateCaptured = true;
        }

        private void PrepareSourceActorAnimationForSequence()
        {
            sourceActorStateCaptured = false;
            sourceActorCueApplied = false;
            if (sequenceProfile == null
                || sequenceProfile.SourceActorAnimations.Length == 0
                || !HasSourceActorBindingSetup())
            {
                return;
            }

            if (sourceActorBindings != null && sourceActorBindings.Length > 0)
            {
                originalSourceActorBindingActiveStates = new bool[sourceActorBindings.Length];
                Dictionary<GameObject, bool> capturedActiveStates =
                    new Dictionary<GameObject, bool>();
                for (int i = 0; i < sourceActorBindings.Length; i++)
                {
                    GameObject visibilityRoot = sourceActorBindings[i].VisibilityRoot;
                    if (visibilityRoot == null)
                    {
                        continue;
                    }

                    if (!capturedActiveStates.TryGetValue(visibilityRoot, out bool wasActive))
                    {
                        wasActive = visibilityRoot.activeSelf;
                        capturedActiveStates.Add(visibilityRoot, wasActive);
                    }

                    originalSourceActorBindingActiveStates[i] = wasActive;
                    visibilityRoot.SetActive(false);
                }
            }
            else
            {
                GameObject sourceVisibilityRoot = ResolveSourceActorVisibilityRoot();
                if (sourceVisibilityRoot == null)
                {
                    return;
                }

                originalSourceActorActive = sourceVisibilityRoot.activeSelf;
                sourceVisibilityRoot.SetActive(false);
            }

            originalPrimaryActorActive =
                primaryActorRootHiddenDuringSourceActorAnimation == null
                || primaryActorRootHiddenDuringSourceActorAnimation.activeSelf;
            sourceActorStateCaptured = true;
            SetPrimaryActorActiveForSourceActor(false);
        }

        private static bool ShouldApplyBodyControllerOverride(CinematicSequenceProfile.ActorRole role)
        {
            return role == CinematicSequenceProfile.ActorRole.Inori
                || role == CinematicSequenceProfile.ActorRole.Player;
        }

        private void RestoreBodyControllersIfNeeded()
        {
            if (!bodyControllerStateCaptured || actorBindings == null || originalBodyControllers == null)
            {
                return;
            }

            int count = Mathf.Min(actorBindings.Length, originalBodyControllers.Length);
            for (int i = 0; i < count; i++)
            {
                Animator animator = actorBindings[i].BodyAnimator;
                if (animator == null)
                {
                    continue;
                }

                animator.runtimeAnimatorController = originalBodyControllers[i];
            }

            bodyControllerStateCaptured = false;
            originalBodyControllers = Array.Empty<RuntimeAnimatorController>();
        }

        private void RestoreSourceActorStateIfNeeded()
        {
            if (!sourceActorStateCaptured)
            {
                return;
            }

            if (sourceActorBindings != null && sourceActorBindings.Length > 0)
            {
                int count = Mathf.Min(sourceActorBindings.Length, originalSourceActorBindingActiveStates.Length);
                for (int i = 0; i < count; i++)
                {
                    GameObject visibilityRoot = sourceActorBindings[i].VisibilityRoot;
                    if (visibilityRoot != null)
                    {
                        visibilityRoot.SetActive(originalSourceActorBindingActiveStates[i]);
                    }
                }
            }
            else
            {
                GameObject sourceVisibilityRoot = ResolveSourceActorVisibilityRoot();
                if (sourceVisibilityRoot != null)
                {
                    sourceVisibilityRoot.SetActive(originalSourceActorActive);
                }
            }

            if (primaryActorRootHiddenDuringSourceActorAnimation != null)
            {
                primaryActorRootHiddenDuringSourceActorAnimation.SetActive(originalPrimaryActorActive);
            }

            sourceActorCueApplied = false;
            sourceActorStateCaptured = false;
            originalSourceActorBindingActiveStates = Array.Empty<bool>();
        }

        private void DisablePlaybackLockedBehaviours()
        {
            if (behavioursDisabledDuringPlayback == null || behavioursDisabledDuringPlayback.Length == 0)
            {
                playbackLockStateCaptured = false;
                originalPlaybackLockEnabledStates = Array.Empty<bool>();
                return;
            }

            originalPlaybackLockEnabledStates = new bool[behavioursDisabledDuringPlayback.Length];
            playbackLockStateCaptured = true;
            for (int i = 0; i < behavioursDisabledDuringPlayback.Length; i++)
            {
                Behaviour behaviour = behavioursDisabledDuringPlayback[i];
                if (behaviour == null || behaviour == this)
                {
                    continue;
                }

                originalPlaybackLockEnabledStates[i] = behaviour.enabled;
                if (behaviour.enabled)
                {
                    behaviour.enabled = false;
                }
            }
        }

        private void RestorePlaybackLockedBehavioursIfNeeded()
        {
            if (!playbackLockStateCaptured)
            {
                return;
            }

            int count = Mathf.Min(
                behavioursDisabledDuringPlayback != null ? behavioursDisabledDuringPlayback.Length : 0,
                originalPlaybackLockEnabledStates != null ? originalPlaybackLockEnabledStates.Length : 0);
            for (int i = 0; i < count; i++)
            {
                Behaviour behaviour = behavioursDisabledDuringPlayback[i];
                if (behaviour == null || behaviour == this)
                {
                    continue;
                }

                behaviour.enabled = originalPlaybackLockEnabledStates[i];
            }

            playbackLockStateCaptured = false;
            originalPlaybackLockEnabledStates = Array.Empty<bool>();
        }

        private void RestoreCameraStateIfNeeded()
        {
            if (cameraStateCaptured)
            {
                Camera camera = ResolveCinematicCamera();
                if (camera != null && ShouldRestoreCameraAfterSequence())
                {
                    camera.transform.SetPositionAndRotation(originalCameraPosition, originalCameraRotation);
                    camera.fieldOfView = originalCameraFieldOfView;
                }
            }

            if (cameraControllerStateCaptured && cameraController != null)
            {
                cameraController.enabled = originalCameraControllerEnabled;
            }

            cameraStateCaptured = false;
            cameraControllerStateCaptured = false;
        }

        private bool SequenceUsesDrivenCameraPose()
        {
            if (sequenceProfile.SourceCameraAnimations.Length > 0)
            {
                return true;
            }

            CinematicSequenceProfile.CameraCue[] cameraCues = sequenceProfile.CameraCues;
            for (int i = 0; i < cameraCues.Length; i++)
            {
                if (cameraCues[i].Enabled && cameraCues[i].DriveCameraPose)
                {
                    return true;
                }
            }

            return false;
        }

        private bool ShouldRestoreCameraAfterSequence()
        {
            return sequenceProfile == null
                || !sequenceProfile.GameplayHandoff.Enabled
                || sequenceProfile.GameplayHandoff.RestoreCamera;
        }

        private bool PrewarmSourceCameraAnimation(Camera camera)
        {
            if (camera == null || sequenceProfile == null)
            {
                return false;
            }

            CinematicSequenceProfile.SourceCameraAnimationCue[] cues =
                sequenceProfile.SourceCameraAnimations;
            for (int i = 0; i < cues.Length; i++)
            {
                if (cues[i].Enabled && cues[i].StartSeconds <= 0.01f)
                {
                    return SampleSourceCameraAnimation(0f, camera);
                }
            }

            return false;
        }

        private bool SampleSourceCameraAnimation(float elapsedSeconds)
        {
            return SampleSourceCameraAnimation(elapsedSeconds, ResolveCinematicCamera());
        }

        private bool SampleSourceCameraAnimation(float elapsedSeconds, Camera camera)
        {
            if (!driveCameraTransformFromProfile
                || sequenceProfile == null
                || camera == null
                || sequenceProfile.SourceCameraAnimations.Length == 0)
            {
                return false;
            }

            CinematicSequenceProfile.SourceCameraAnimationCue[] cues =
                sequenceProfile.SourceCameraAnimations;
            for (int i = 0; i < cues.Length; i++)
            {
                CinematicSequenceProfile.SourceCameraAnimationCue cue = cues[i];
                if (!IsSourceCameraCueActive(cues, i, elapsedSeconds))
                {
                    continue;
                }

                if (activeCameraPoseRoutine != null)
                {
                    StopCoroutine(activeCameraPoseRoutine);
                    activeCameraPoseRoutine = null;
                }

                if (!TryResolveSourceCameraBinding(cue, out SourceCameraBinding binding))
                {
                    return false;
                }

                float localSeconds = Mathf.Clamp(elapsedSeconds - cue.StartSeconds, 0f, cue.DurationSeconds);
                cue.Clip.SampleAnimation(binding.RigRoot, cue.ClipInSeconds + localSeconds);

                Transform sourceTransform = binding.CameraTransform;
                Camera sourceCamera = binding.CameraComponent;
                float sourceFieldOfView = sourceCamera != null ? sourceCamera.fieldOfView : camera.fieldOfView;
                ApplyCameraPose(camera, sourceTransform.position, sourceTransform.rotation, sourceFieldOfView);
                lastCameraCueId = cue.CueId;
                totalCameraCueCount = Mathf.Max(totalCameraCueCount, 1);
                sourceCameraCueApplied = true;
                return true;
            }

            ReleaseSourceCameraIfFinished(elapsedSeconds, cues);
            return false;
        }

        private void ReleaseSourceCameraIfFinished(
            float elapsedSeconds,
            CinematicSequenceProfile.SourceCameraAnimationCue[] cues)
        {
            float finalCueEndSeconds = ResolveFinalSourceCameraCueEndSeconds(cues);
            if (!sourceCameraCueApplied
                || elapsedSeconds <= finalCueEndSeconds
                || HasAuthoredCameraCueAfter(finalCueEndSeconds))
            {
                return;
            }

            RestoreCameraStateIfNeeded();
            sourceCameraCueApplied = false;
        }

        private bool HasAuthoredCameraCueAfter(float seconds)
        {
            if (sequenceProfile == null)
            {
                return false;
            }

            CinematicSequenceProfile.CameraCue[] cameraCues = sequenceProfile.CameraCues;
            for (int i = 0; i < cameraCues.Length; i++)
            {
                if (cameraCues[i].Enabled && cameraCues[i].StartSeconds >= seconds)
                {
                    return true;
                }
            }

            return false;
        }

        private float ResolveFinalSourceCameraCueEndSeconds(
            CinematicSequenceProfile.SourceCameraAnimationCue[] cues)
        {
            float endSeconds = 0f;
            for (int i = 0; i < cues.Length; i++)
            {
                if (cues[i].Enabled)
                {
                    endSeconds = Mathf.Max(endSeconds, cues[i].EndSeconds);
                }
            }

            return endSeconds;
        }

        private bool SampleSourceActorAnimation(float elapsedSeconds)
        {
            if (sequenceProfile == null
                || sequenceProfile.SourceActorAnimations.Length == 0
                || !HasSourceActorBindingSetup())
            {
                if (sourceActorCueApplied)
                {
                    SetSourceActorCueActive(false);
                }

                return false;
            }

            CinematicSequenceProfile.SourceActorAnimationCue[] cues =
                sequenceProfile.SourceActorAnimations;
            activeSourceActorCueIds.Clear();
            for (int i = 0; i < cues.Length; i++)
            {
                CinematicSequenceProfile.SourceActorAnimationCue cue = cues[i];
                if (!IsSourceActorCueActive(cues, i, elapsedSeconds))
                {
                    continue;
                }

                if (TryResolveSourceActorBinding(cue, out _))
                {
                    activeSourceActorCueIds.Add(cue.CueId);
                }
            }

            if (activeSourceActorCueIds.Count == 0)
            {
                if (sourceActorCueApplied)
                {
                    SetSourceActorCueActive(false);
                }

                return false;
            }

            SetSourceActorCuesActive(activeSourceActorCueIds);
            for (int i = 0; i < cues.Length; i++)
            {
                CinematicSequenceProfile.SourceActorAnimationCue cue = cues[i];
                if (!activeSourceActorCueIds.Contains(cue.CueId)
                    || !TryResolveSourceActorBinding(cue, out SourceActorBinding binding))
                {
                    continue;
                }

                float localSeconds = Mathf.Clamp(elapsedSeconds - cue.StartSeconds, 0f, cue.DurationSeconds);
                cue.Clip.SampleAnimation(binding.RigRoot, cue.ClipInSeconds + localSeconds);
            }

            return true;
        }

        private bool SampleSourceActorGrade(float elapsedSeconds)
        {
            if (sequenceProfile == null
                || sequenceProfile.SourceActorGrades.Length == 0
                || !HasSourceActorBindingSetup())
            {
                ClearSourceActorGrade();
                return false;
            }

            if (!TryResolveSourceActorGrade(elapsedSeconds, out CinematicSequenceProfile.SourceActorGradeCue cue, out Color color))
            {
                ClearSourceActorGrade();
                return false;
            }

            if (sourceActorGradeBlock == null)
            {
                sourceActorGradeBlock = new MaterialPropertyBlock();
            }

            sourceActorGradeBlock.Clear();
            sourceActorGradeBlock.SetColor(SourceActorGradeBaseColorId, color);
            sourceActorGradeBlock.SetColor(SourceActorGradeColorId, color);
            sourceActorGradeBlock.SetColor(SourceActorGradeEmissionColorId, Color.black);
            sourceActorGradeBlock.SetColor(SourceActorGradeFirstShadeColorId, color);
            sourceActorGradeBlock.SetColor(SourceActorGradeSecondShadeColorId, color);

            bool applied = ApplySourceActorGradeToRenderers(cue.CueId, sourceActorGradeBlock);
            if (!applied)
            {
                ClearSourceActorGrade();
                return false;
            }

            sourceActorGradeApplied = true;
            return true;
        }

        private bool TryResolveSourceActorGrade(
            float elapsedSeconds,
            out CinematicSequenceProfile.SourceActorGradeCue selectedCue,
            out Color selectedColor)
        {
            selectedCue = default;
            selectedColor = Color.white;
            CinematicSequenceProfile.SourceActorGradeCue[] cues = sequenceProfile.SourceActorGrades;
            int selectedIndex = -1;
            float selectedStartSeconds = -1f;
            for (int i = 0; i < cues.Length; i++)
            {
                CinematicSequenceProfile.SourceActorGradeCue cue = cues[i];
                if (!cue.Enabled || elapsedSeconds < cue.StartSeconds)
                {
                    continue;
                }

                if (selectedIndex < 0 || cue.StartSeconds >= selectedStartSeconds)
                {
                    selectedIndex = i;
                    selectedStartSeconds = cue.StartSeconds;
                }
            }

            if (selectedIndex < 0)
            {
                return false;
            }

            selectedCue = cues[selectedIndex];
            selectedColor = selectedCue.Evaluate(elapsedSeconds);
            return true;
        }

        private bool ApplySourceActorGradeToRenderers(string cueId, MaterialPropertyBlock propertyBlock)
        {
            bool anyApplied = false;
            if (sourceActorBindings != null && sourceActorBindings.Length > 0)
            {
                for (int i = 0; i < sourceActorBindings.Length; i++)
                {
                    SourceActorBinding binding = sourceActorBindings[i];
                    if (!binding.IsValid || !ShouldGradeSourceActorBinding(binding, cueId))
                    {
                        continue;
                    }

                    anyApplied |= ApplySourceActorGradeToRoot(binding.VisibilityRoot, propertyBlock);
                }

                return anyApplied;
            }

            if (sourceActorRigRoot == null)
            {
                return false;
            }

            return ApplySourceActorGradeToRoot(ResolveSourceActorVisibilityRoot(), propertyBlock);
        }

        private bool ShouldGradeSourceActorBinding(SourceActorBinding binding, string cueId)
        {
            if (string.IsNullOrWhiteSpace(cueId))
            {
                return true;
            }

            return binding.Matches(cueId);
        }

        private bool ApplySourceActorGradeToRoot(GameObject root, MaterialPropertyBlock propertyBlock)
        {
            if (root == null)
            {
                return false;
            }

            bool anyApplied = false;
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(includeInactive: true);
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null || IsSourceActorGradeExcluded(renderer.transform))
                {
                    continue;
                }

                renderer.SetPropertyBlock(propertyBlock);
                anyApplied = true;
            }

            return anyApplied;
        }

        private void ClearSourceActorGrade()
        {
            if (!sourceActorGradeApplied)
            {
                return;
            }

            if (sourceActorBindings != null && sourceActorBindings.Length > 0)
            {
                for (int i = 0; i < sourceActorBindings.Length; i++)
                {
                    SourceActorBinding binding = sourceActorBindings[i];
                    if (binding.IsValid)
                    {
                        ClearSourceActorGradeFromRoot(binding.VisibilityRoot);
                    }
                }
            }
            else
            {
                ClearSourceActorGradeFromRoot(ResolveSourceActorVisibilityRoot());
            }

            sourceActorGradeApplied = false;
        }

        private void ClearSourceActorGradeFromRoot(GameObject root)
        {
            if (root == null)
            {
                return;
            }

            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(includeInactive: true);
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null || IsSourceActorGradeExcluded(renderer.transform))
                {
                    continue;
                }

                renderer.SetPropertyBlock(null);
            }
        }

        private bool SampleScreenFade(float elapsedSeconds)
        {
            if (sequenceProfile == null
                || sequenceProfile.ScreenFadeCues.Length == 0
                || screenFadeCanvasGroup == null)
            {
                ClearScreenFade();
                return false;
            }

            if (!TryResolveScreenFadeCue(
                    elapsedSeconds,
                    out CinematicSequenceProfile.ScreenFadeCue cue,
                    out float alpha))
            {
                ClearScreenFade();
                return false;
            }

            if (screenFadeImage != null)
            {
                Color color = cue.Color;
                color.a = 1f;
                screenFadeImage.color = color;
                screenFadeImage.enabled = alpha > 0.0001f;
            }

            screenFadeCanvasGroup.alpha = Mathf.Clamp01(alpha);
            screenFadeCanvasGroup.interactable = false;
            screenFadeCanvasGroup.blocksRaycasts = false;
            return true;
        }

        private bool TryResolveScreenFadeCue(
            float elapsedSeconds,
            out CinematicSequenceProfile.ScreenFadeCue selectedCue,
            out float selectedAlpha)
        {
            selectedCue = default;
            selectedAlpha = 0f;
            CinematicSequenceProfile.ScreenFadeCue[] cues = sequenceProfile.ScreenFadeCues;
            int selectedIndex = -1;
            float selectedStartSeconds = -1f;
            for (int i = 0; i < cues.Length; i++)
            {
                CinematicSequenceProfile.ScreenFadeCue cue = cues[i];
                if (!cue.Enabled || elapsedSeconds < cue.StartSeconds)
                {
                    continue;
                }

                if (selectedIndex < 0 || cue.StartSeconds >= selectedStartSeconds)
                {
                    selectedIndex = i;
                    selectedStartSeconds = cue.StartSeconds;
                }
            }

            if (selectedIndex < 0)
            {
                return false;
            }

            selectedCue = cues[selectedIndex];
            selectedAlpha = selectedCue.EvaluateAlpha(elapsedSeconds);
            return true;
        }

        private void ClearScreenFade()
        {
            if (screenFadeCanvasGroup == null && screenFadeImage == null)
            {
                return;
            }

            if (screenFadeCanvasGroup != null)
            {
                screenFadeCanvasGroup.alpha = 0f;
                screenFadeCanvasGroup.interactable = false;
                screenFadeCanvasGroup.blocksRaycasts = false;
            }

            if (screenFadeImage != null)
            {
                Color color = screenFadeImage.color;
                color.a = 1f;
                screenFadeImage.color = color;
                screenFadeImage.enabled = false;
            }
        }

        private bool IsSourceActorGradeExcluded(Transform candidate)
        {
            if (candidate == null || sourceActorGradeExcludedRoots == null)
            {
                return false;
            }

            for (int i = 0; i < sourceActorGradeExcludedRoots.Length; i++)
            {
                Transform excludedRoot = sourceActorGradeExcludedRoots[i];
                if (excludedRoot != null && candidate.IsChildOf(excludedRoot))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsSourceCameraCueActive(
            CinematicSequenceProfile.SourceCameraAnimationCue[] cues,
            int cueIndex,
            float elapsedSeconds)
        {
            CinematicSequenceProfile.SourceCameraAnimationCue cue = cues[cueIndex];
            if (!cue.Enabled || elapsedSeconds < cue.StartSeconds)
            {
                return false;
            }

            if (elapsedSeconds < cue.EndSeconds)
            {
                return true;
            }

            return Mathf.Abs(elapsedSeconds - cue.EndSeconds) <= 0.0001f
                && !HasLaterSourceCameraCue(cues, cueIndex);
        }

        private static bool IsSourceActorCueActive(
            CinematicSequenceProfile.SourceActorAnimationCue[] cues,
            int cueIndex,
            float elapsedSeconds)
        {
            CinematicSequenceProfile.SourceActorAnimationCue cue = cues[cueIndex];
            if (!cue.Enabled || elapsedSeconds < cue.StartSeconds)
            {
                return false;
            }

            if (elapsedSeconds < cue.EndSeconds)
            {
                return true;
            }

            return Mathf.Abs(elapsedSeconds - cue.EndSeconds) <= 0.0001f
                && !HasLaterSourceActorCue(cues, cueIndex);
        }

        private static bool HasLaterSourceCameraCue(
            CinematicSequenceProfile.SourceCameraAnimationCue[] cues,
            int cueIndex)
        {
            float startSeconds = cues[cueIndex].StartSeconds;
            for (int i = 0; i < cues.Length; i++)
            {
                if (i != cueIndex && cues[i].Enabled && cues[i].StartSeconds > startSeconds)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasLaterSourceActorCue(
            CinematicSequenceProfile.SourceActorAnimationCue[] cues,
            int cueIndex)
        {
            float startSeconds = cues[cueIndex].StartSeconds;
            for (int i = 0; i < cues.Length; i++)
            {
                if (i != cueIndex && cues[i].Enabled && cues[i].StartSeconds > startSeconds)
                {
                    return true;
                }
            }

            return false;
        }

        private GameObject ResolveSourceActorVisibilityRoot()
        {
            return sourceActorVisibilityRoot != null ? sourceActorVisibilityRoot : sourceActorRigRoot;
        }

        private bool HasSourceActorBindingSetup()
        {
            if (sourceActorBindings != null)
            {
                for (int i = 0; i < sourceActorBindings.Length; i++)
                {
                    if (sourceActorBindings[i].IsValid)
                    {
                        return true;
                    }
                }
            }

            return sourceActorRigRoot != null;
        }

        private bool TryResolveSourceCameraBinding(
            CinematicSequenceProfile.SourceCameraAnimationCue cue,
            out SourceCameraBinding binding)
        {
            if (sourceCameraBindings != null)
            {
                for (int i = 0; i < sourceCameraBindings.Length; i++)
                {
                    if (sourceCameraBindings[i].Matches(cue.CueId) && sourceCameraBindings[i].IsValid)
                    {
                        binding = sourceCameraBindings[i];
                        return true;
                    }
                }
            }

            Camera fallbackCamera = ResolveSourceCameraComponent();
            Transform fallbackTransform = ResolveSourceCameraTransform(fallbackCamera);
            if (sourceCameraRigRoot == null || fallbackTransform == null)
            {
                binding = default;
                return false;
            }

            binding = new SourceCameraBinding(string.Empty, sourceCameraRigRoot, fallbackTransform, fallbackCamera);
            return true;
        }

        private bool TryResolveSourceActorBinding(
            CinematicSequenceProfile.SourceActorAnimationCue cue,
            out SourceActorBinding binding)
        {
            if (sourceActorBindings != null)
            {
                for (int i = 0; i < sourceActorBindings.Length; i++)
                {
                    if (sourceActorBindings[i].Matches(cue.CueId) && sourceActorBindings[i].IsValid)
                    {
                        binding = sourceActorBindings[i];
                        return true;
                    }
                }
            }

            if (sourceActorRigRoot == null)
            {
                binding = default;
                return false;
            }

            binding = new SourceActorBinding(string.Empty, sourceActorRigRoot, ResolveSourceActorVisibilityRoot());
            return true;
        }

        private void SetSourceActorCueActive(bool active)
        {
            if (sourceActorBindings != null && sourceActorBindings.Length > 0)
            {
                SetSourceActorCueActive(active ? string.Empty : null);
                return;
            }

            GameObject sourceVisibilityRoot = ResolveSourceActorVisibilityRoot();
            if (sourceVisibilityRoot != null && sourceVisibilityRoot.activeSelf != active)
            {
                sourceVisibilityRoot.SetActive(active);
            }

            SetPrimaryActorActiveForSourceActor(active);
            sourceActorCueApplied = active;
        }

        private void SetSourceActorCueActive(string activeCueId)
        {
            if (sourceActorBindings == null || sourceActorBindings.Length == 0)
            {
                SetSourceActorCueActive(!string.IsNullOrWhiteSpace(activeCueId));
                return;
            }

            activeSourceActorCueIds.Clear();
            if (!string.IsNullOrWhiteSpace(activeCueId))
            {
                activeSourceActorCueIds.Add(activeCueId);
            }

            SetSourceActorCuesActive(activeSourceActorCueIds);
        }

        private void SetSourceActorCuesActive(HashSet<string> activeCueIds)
        {
            if (sourceActorBindings == null || sourceActorBindings.Length == 0)
            {
                SetSourceActorCueActive(activeCueIds != null && activeCueIds.Count > 0);
                return;
            }

            bool anyActive = false;
            for (int i = 0; i < sourceActorBindings.Length; i++)
            {
                GameObject visibilityRoot = sourceActorBindings[i].VisibilityRoot;
                if (visibilityRoot == null)
                {
                    continue;
                }

                bool active = IsSourceActorVisibilityRootActive(i, activeCueIds);
                if (visibilityRoot.activeSelf != active)
                {
                    visibilityRoot.SetActive(active);
                }

                anyActive |= active;
            }

            SetPrimaryActorActiveForSourceActor(anyActive);
            sourceActorCueApplied = anyActive;
        }

        private bool IsSourceActorVisibilityRootActive(int bindingIndex, HashSet<string> activeCueIds)
        {
            if (activeCueIds == null || activeCueIds.Count == 0)
            {
                return false;
            }

            GameObject visibilityRoot = sourceActorBindings[bindingIndex].VisibilityRoot;
            if (visibilityRoot == null)
            {
                return false;
            }

            for (int i = 0; i < sourceActorBindings.Length; i++)
            {
                if (sourceActorBindings[i].VisibilityRoot == visibilityRoot
                    && activeCueIds.Contains(sourceActorBindings[i].CueId))
                {
                    return true;
                }
            }

            return false;
        }

        private void SetPrimaryActorActiveForSourceActor(bool sourceActorActive)
        {
            if (primaryActorRootHiddenDuringSourceActorAnimation != null)
            {
                bool primaryShouldBeActive =
                    sourceActorActive ? false : (!sourceActorStateCaptured || originalPrimaryActorActive);
                if (primaryActorRootHiddenDuringSourceActorAnimation.activeSelf != primaryShouldBeActive)
                {
                    primaryActorRootHiddenDuringSourceActorAnimation.SetActive(primaryShouldBeActive);
                }
            }
        }

        private Camera ResolveSourceCameraComponent()
        {
            if (sourceCameraComponent != null)
            {
                return sourceCameraComponent;
            }

            if (sourceCameraRigRoot != null)
            {
                sourceCameraComponent = sourceCameraRigRoot.GetComponentInChildren<Camera>(includeInactive: true);
            }

            return sourceCameraComponent;
        }

        private Transform ResolveSourceCameraTransform(Camera sourceCamera)
        {
            if (sourceCameraTransform != null)
            {
                return sourceCameraTransform;
            }

            sourceCameraTransform = sourceCamera != null ? sourceCamera.transform : null;
            return sourceCameraTransform;
        }

        private bool TryDriveCameraPose(CinematicSequenceProfile.CameraCue cue)
        {
            Camera camera = ResolveCinematicCamera();
            if (camera == null)
            {
                return false;
            }

            if (!TryResolveCameraPose(cue, camera, out Vector3 targetPosition, out Quaternion targetRotation, out float targetFieldOfView))
            {
                return false;
            }

            if (activeCameraPoseRoutine != null)
            {
                StopCoroutine(activeCameraPoseRoutine);
                activeCameraPoseRoutine = null;
            }

            if (forceImmediateCameraPoseForReviewSample
                || cue.BlendKind == CinematicSequenceProfile.CameraBlendKind.Cut
                || cue.DurationSeconds <= 0.02f)
            {
                ApplyCameraPose(camera, targetPosition, targetRotation, targetFieldOfView);
                return true;
            }

            activeCameraPoseRoutine = StartCoroutine(BlendCameraPose(
                camera,
                targetPosition,
                targetRotation,
                targetFieldOfView,
                cue.DurationSeconds,
                cue.BlendKind));
            return true;
        }

        private void PrewarmOpeningCameraPose(Camera camera)
        {
            if (sequenceProfile == null || camera == null)
            {
                return;
            }

            CinematicSequenceProfile.CameraCue[] cameraCues = sequenceProfile.CameraCues;
            int openingCueIndex = -1;
            float openingStart = float.MaxValue;
            for (int i = 0; i < cameraCues.Length; i++)
            {
                CinematicSequenceProfile.CameraCue cue = cameraCues[i];
                if (!cue.Enabled || !cue.DriveCameraPose || cue.StartSeconds > 0.01f)
                {
                    continue;
                }

                if (cue.StartSeconds < openingStart)
                {
                    openingStart = cue.StartSeconds;
                    openingCueIndex = i;
                }
            }

            if (openingCueIndex < 0)
            {
                return;
            }

            if (TryResolveCameraPose(
                cameraCues[openingCueIndex],
                camera,
                out Vector3 targetPosition,
                out Quaternion targetRotation,
                out float targetFieldOfView))
            {
                ApplyCameraPose(camera, targetPosition, targetRotation, targetFieldOfView);
            }
        }

        private bool TryResolveCameraPose(
            CinematicSequenceProfile.CameraCue cue,
            Camera camera,
            out Vector3 targetPosition,
            out Quaternion targetRotation,
            out float targetFieldOfView)
        {
            targetPosition = default;
            targetRotation = default;
            targetFieldOfView = default;
            if (!cue.DriveCameraPose || camera == null)
            {
                return false;
            }

            Transform space = cueSpace != null ? cueSpace : transform;
            targetPosition = space.TransformPoint(cue.CameraLocalPosition);
            Vector3 targetLookAt = space.TransformPoint(cue.LookAtLocalPosition);
            targetRotation = ResolveLookRotation(camera.transform, targetPosition, targetLookAt);
            targetFieldOfView = cue.FieldOfView > 0f
                ? cue.FieldOfView
                : Mathf.Clamp(camera.fieldOfView + cue.FieldOfViewDelta, 1f, 179f);
            return true;
        }

        private IEnumerator BlendCameraPose(
            Camera camera,
            Vector3 targetPosition,
            Quaternion targetRotation,
            float targetFieldOfView,
            float durationSeconds,
            CinematicSequenceProfile.CameraBlendKind blendKind)
        {
            Vector3 startPosition = camera.transform.position;
            Quaternion startRotation = camera.transform.rotation;
            float startFieldOfView = camera.fieldOfView;
            float elapsed = 0f;

            while (elapsed < durationSeconds)
            {
                float deltaTime = ResolvePlaybackDeltaSeconds();
                elapsed += Mathf.Max(0f, deltaTime);
                float t = Mathf.Clamp01(elapsed / Mathf.Max(0.01f, durationSeconds));
                float eased = EvaluateCameraEase(t, blendKind);
                ApplyCameraPose(
                    camera,
                    Vector3.Lerp(startPosition, targetPosition, eased),
                    Quaternion.Slerp(startRotation, targetRotation, eased),
                    Mathf.Lerp(startFieldOfView, targetFieldOfView, eased));
                yield return null;
            }

            ApplyCameraPose(camera, targetPosition, targetRotation, targetFieldOfView);
            activeCameraPoseRoutine = null;
        }

        private Camera ResolveCinematicCamera()
        {
            if (cinematicCamera != null)
            {
                return cinematicCamera;
            }

            if (cameraController != null)
            {
                cinematicCamera = cameraController.GetComponent<Camera>();
                if (cinematicCamera != null)
                {
                    return cinematicCamera;
                }
            }

            cinematicCamera = Camera.main;
            return cinematicCamera;
        }

        private static void ApplyCameraPose(Camera camera, Vector3 position, Quaternion rotation, float fieldOfView)
        {
            camera.transform.SetPositionAndRotation(position, rotation);
            camera.fieldOfView = Mathf.Clamp(fieldOfView, 1f, 179f);
        }

        private static Quaternion ResolveLookRotation(Transform currentCamera, Vector3 position, Vector3 lookAt)
        {
            Vector3 forward = lookAt - position;
            if (forward.sqrMagnitude < 0.0001f)
            {
                forward = currentCamera != null ? currentCamera.forward : Vector3.forward;
            }

            return Quaternion.LookRotation(forward.normalized, Vector3.up);
        }

        private static float EvaluateCameraEase(float t, CinematicSequenceProfile.CameraBlendKind blendKind)
        {
            float clamped = Mathf.Clamp01(t);
            switch (blendKind)
            {
                case CinematicSequenceProfile.CameraBlendKind.PushIn:
                    return clamped * clamped * (3f - 2f * clamped);
                case CinematicSequenceProfile.CameraBlendKind.PullBack:
                    return 1f - Mathf.Pow(1f - clamped, 3f);
                case CinematicSequenceProfile.CameraBlendKind.Reframe:
                case CinematicSequenceProfile.CameraBlendKind.GameplayMatch:
                case CinematicSequenceProfile.CameraBlendKind.Ease:
                    return clamped * clamped * (3f - 2f * clamped);
                default:
                    return clamped;
            }
        }

        private Vector3 ResolveDefaultDirection()
        {
            Transform space = cueSpace != null ? cueSpace : transform;
            Vector3 forward = Vector3.ProjectOnPlane(space.forward, Vector3.up);
            return forward.sqrMagnitude > 0.0001f ? forward.normalized : Vector3.forward;
        }

        private void ResetCounters()
        {
            totalCameraCueCount = 0;
            totalActorCueCount = 0;
            totalBoundActorCueCount = 0;
            totalVfxCueCount = 0;
            totalTutorialCueCount = 0;
            lastCameraCueId = string.Empty;
            lastActorCueId = string.Empty;
            lastVfxCueId = string.Empty;
            lastTutorialCueId = string.Empty;
            gameplayHandoffReached = false;
            if (tutorialPromptPresenter != null)
            {
                tutorialPromptPresenter.HidePrompt();
            }
        }
    }
}
