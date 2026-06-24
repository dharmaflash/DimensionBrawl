using System;
using System.Collections;
using DimensionBrawl.Combat;
using UnityEngine;

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

        [Header("Action Cue Bridge")]
        [SerializeField] private bool playLinkedActionCueOnStart;
        [SerializeField] private ActionCinematicCueDirector linkedActionCueDirector;
        [SerializeField] private ActionCinematicCueProfile.CueKind linkedActionCueKind =
            ActionCinematicCueProfile.CueKind.SkillCutIn;
        [SerializeField, Min(1)] private int linkedActionCueTier = 1;

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
            if (profile == null || activeRoutine != null)
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
                if (animator == null)
                {
                    continue;
                }

                CinematicSequenceProfile.ActorCue? selectedCue = null;
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

                    selectedCue = cue;
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
            if (sequenceProfile == null || activeRoutine != null)
            {
                return false;
            }

            ResetCounters();
            PrepareCameraForSequence();
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

            while (elapsed < duration)
            {
                float deltaTime = sequenceProfile.UseUnscaledClock ? Time.unscaledDeltaTime : Time.deltaTime;
                elapsed += Mathf.Max(0f, deltaTime);
                DispatchDueCues(
                    elapsed,
                    planarDirection,
                    cameraPlayed,
                    actorPlayed,
                    vfxPlayed,
                    tutorialPlayed,
                    ref handoffPlayed);
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
            if (animator == null || string.IsNullOrWhiteSpace(stateName))
            {
                return;
            }

            float transitionSeconds = Mathf.Clamp(durationSeconds * 0.12f, 0.04f, 0.16f);
            animator.CrossFadeInFixedTime(stateName, transitionSeconds);
        }

        private static void TriggerAnimator(Animator animator, string triggerName)
        {
            if (animator == null || string.IsNullOrWhiteSpace(triggerName) || !HasAnimatorTrigger(animator, triggerName))
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

            PrewarmOpeningCameraPose(camera);
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
                float deltaTime = sequenceProfile != null && sequenceProfile.UseUnscaledClock
                    ? Time.unscaledDeltaTime
                    : Time.deltaTime;
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
