using System.Reflection;
using DimensionBrawl.Combat;
using DimensionBrawl.Player;
using DimensionBrawl.Presentation;
using DimensionBrawl.UI;
using UnityEngine;
using UnityEngine.Playables;

namespace DimensionBrawl.LevelDesign
{
    [DisallowMultipleComponent]
    public sealed class OlympusCorridorCombatFlowController : MonoBehaviour
    {
        private enum FlowPhase
        {
            WaitingForIntroHandoff,
            IntroSwordGate,
            WaitingForStairEntry,
            CorridorCombat
        }

        [Header("Intro Handoff")]
        [SerializeField] private PlayableDirector introDirector;
        [SerializeField, Min(0f)] private double introHandoffSeconds = 36.5d;
        [SerializeField] private Camera[] introCamerasToDisable = System.Array.Empty<Camera>();
        [SerializeField] private AudioListener[] introAudioListenersToDisable = System.Array.Empty<AudioListener>();
        [SerializeField] private Behaviour[] cutsceneBehavioursToDisableOnHandoff =
            System.Array.Empty<Behaviour>();
        [SerializeField] private GameObject[] cutsceneRootsToDisableOnHandoff = System.Array.Empty<GameObject>();
        [SerializeField] private GameObject[] handoffRoots = System.Array.Empty<GameObject>();
        [SerializeField] private GameObject[] alwaysDisabledRoots = System.Array.Empty<GameObject>();
        [SerializeField] private ActionCameraController combatCameraController;
        [SerializeField] private Transform combatCameraHandoffPose;

        [Header("Sword Gate")]
        [SerializeField] private GameObject introSwordGateRoot;
        [SerializeField] private CombatHealth[] introSwordEnemies = System.Array.Empty<CombatHealth>();
        [SerializeField] private Behaviour[] introSwordEnemyGameplayBehaviours =
            System.Array.Empty<Behaviour>();
        [SerializeField] private Collider[] stairBlockers = System.Array.Empty<Collider>();

        [Header("Handoff UI Reveal")]
        [SerializeField] private BossBarrageLaneReviewHud reviewHud;
        [SerializeField] private BossBarrageLaneReviewMobileHud mobileHud;
        [SerializeField, Min(0f)] private float hudRevealDelaySeconds = 0.08f;
        [SerializeField, Min(0.01f)] private float hudRevealDurationSeconds = 0.18f;

        [Header("Stair To Corridor")]
        [SerializeField] private Transform stairTriggerCenter;
        [SerializeField, Min(0f)] private float stairTriggerRadius = 2.75f;
        [SerializeField] private GameObject[] corridorCombatRoots = System.Array.Empty<GameObject>();
        [SerializeField] private GameObject[] corridorBoundsRoots = System.Array.Empty<GameObject>();
        [SerializeField] private CombatHealth[] corridorTargets = System.Array.Empty<CombatHealth>();

        [Header("Player")]
        [SerializeField] private PlayerMovementController player;
        [SerializeField] private PlayerCombatModeController combatModeController;
        [SerializeField] private PlayerCombatTargetSelector targetSelector;
        [SerializeField] private PlayerRangedBasicAttackAction rangedBasicAttackAction;
        [SerializeField] private PlayerSkill1Action skill1Action;
        [SerializeField] private PlayerSummonSlot1Action summonSlot1Action;
        [SerializeField] private PlayerSupportSummonSlotAction[] supportSummonActions =
            System.Array.Empty<PlayerSupportSummonSlotAction>();

        [Header("Debug Readout")]
        [SerializeField] private FlowPhase phase = FlowPhase.WaitingForIntroHandoff;

        private float hudRevealTimer;
        private bool observedIntroDirectorPlayback;

        public bool IntroGateCleared => CountAlive(introSwordEnemies) == 0;
        public bool CorridorCombatStarted => phase == FlowPhase.CorridorCombat;

        public void Configure(
            PlayableDirector newIntroDirector,
            double newIntroHandoffSeconds,
            Camera[] newIntroCamerasToDisable,
            AudioListener[] newIntroAudioListenersToDisable,
            Behaviour[] newCutsceneBehavioursToDisableOnHandoff,
            GameObject[] newCutsceneRootsToDisableOnHandoff,
            GameObject[] newHandoffRoots,
            GameObject[] newAlwaysDisabledRoots,
            ActionCameraController newCombatCameraController,
            Transform newCombatCameraHandoffPose,
            GameObject newIntroSwordGateRoot,
            CombatHealth[] newIntroSwordEnemies,
            Behaviour[] newIntroSwordEnemyGameplayBehaviours,
            Collider[] newStairBlockers,
            Transform newStairTriggerCenter,
            float newStairTriggerRadius,
            GameObject[] newCorridorCombatRoots,
            GameObject[] newCorridorBoundsRoots,
            CombatHealth[] newCorridorTargets,
            PlayerMovementController newPlayer,
            PlayerCombatModeController newCombatModeController,
            PlayerCombatTargetSelector newTargetSelector,
            PlayerRangedBasicAttackAction newRangedBasicAttackAction,
            PlayerSkill1Action newSkill1Action,
            PlayerSummonSlot1Action newSummonSlot1Action,
            PlayerSupportSummonSlotAction[] newSupportSummonActions,
            BossBarrageLaneReviewHud newReviewHud,
            BossBarrageLaneReviewMobileHud newMobileHud,
            float newHudRevealDelaySeconds,
            float newHudRevealDurationSeconds)
        {
            UnregisterIntroDirectorStoppedHandler();
            introDirector = newIntroDirector;
            RegisterIntroDirectorStoppedHandler();
            introHandoffSeconds = System.Math.Max(0d, newIntroHandoffSeconds);
            introCamerasToDisable = newIntroCamerasToDisable ?? System.Array.Empty<Camera>();
            introAudioListenersToDisable = newIntroAudioListenersToDisable ?? System.Array.Empty<AudioListener>();
            cutsceneBehavioursToDisableOnHandoff =
                newCutsceneBehavioursToDisableOnHandoff ?? System.Array.Empty<Behaviour>();
            cutsceneRootsToDisableOnHandoff =
                newCutsceneRootsToDisableOnHandoff ?? System.Array.Empty<GameObject>();
            handoffRoots = newHandoffRoots ?? System.Array.Empty<GameObject>();
            alwaysDisabledRoots = newAlwaysDisabledRoots ?? System.Array.Empty<GameObject>();
            combatCameraController = newCombatCameraController;
            combatCameraHandoffPose = newCombatCameraHandoffPose;
            introSwordGateRoot = newIntroSwordGateRoot;
            introSwordEnemies = newIntroSwordEnemies ?? System.Array.Empty<CombatHealth>();
            introSwordEnemyGameplayBehaviours =
                newIntroSwordEnemyGameplayBehaviours ?? System.Array.Empty<Behaviour>();
            stairBlockers = newStairBlockers ?? System.Array.Empty<Collider>();
            stairTriggerCenter = newStairTriggerCenter;
            stairTriggerRadius = Mathf.Max(0f, newStairTriggerRadius);
            corridorCombatRoots = newCorridorCombatRoots ?? System.Array.Empty<GameObject>();
            corridorBoundsRoots = newCorridorBoundsRoots ?? System.Array.Empty<GameObject>();
            corridorTargets = newCorridorTargets ?? System.Array.Empty<CombatHealth>();
            player = newPlayer;
            combatModeController = newCombatModeController;
            targetSelector = newTargetSelector;
            rangedBasicAttackAction = newRangedBasicAttackAction;
            skill1Action = newSkill1Action;
            summonSlot1Action = newSummonSlot1Action;
            supportSummonActions = newSupportSummonActions ?? System.Array.Empty<PlayerSupportSummonSlotAction>();
            reviewHud = newReviewHud;
            mobileHud = newMobileHud;
            hudRevealDelaySeconds = Mathf.Max(0f, newHudRevealDelaySeconds);
            hudRevealDurationSeconds = Mathf.Max(0.01f, newHudRevealDurationSeconds);
        }

        private void Awake()
        {
            PrepareInitialState();
        }

        private void OnEnable()
        {
            RegisterIntroDirectorStoppedHandler();
            PrepareInitialState();
        }

        private void OnDisable()
        {
            UnregisterIntroDirectorStoppedHandler();
        }

        private void Update()
        {
            UpdateHudReveal();
            UpdateIntroDirectorPlaybackObservation();

            switch (phase)
            {
                case FlowPhase.WaitingForIntroHandoff:
                    if (IsIntroHandoffReady() || HasIntroDirectorStoppedAfterObservedPlayback())
                    {
                        BeginIntroSwordGate();
                    }
                    break;
                case FlowPhase.IntroSwordGate:
                    if (IntroGateCleared)
                    {
                        BeginWaitingForStairEntry();
                    }
                    break;
                case FlowPhase.WaitingForStairEntry:
                    if (IsPlayerInsideStairTrigger())
                    {
                        BeginCorridorCombat();
                    }
                    break;
                case FlowPhase.CorridorCombat:
                    break;
            }
        }

        private void PrepareInitialState()
        {
            if (phase != FlowPhase.WaitingForIntroHandoff)
            {
                return;
            }

            SetObjectsActive(handoffRoots, false);
            SetObjectActive(player != null ? player.gameObject : null, false);
            SetObjectsActive(alwaysDisabledRoots, false);
            SetHudOpacity(0f);
            hudRevealTimer = -hudRevealDelaySeconds;
            SetBehavioursEnabled(introSwordEnemyGameplayBehaviours, false);
            SetObjectActive(introSwordGateRoot, false);
            SetObjectsActive(corridorCombatRoots, false);
            SetObjectsActive(corridorBoundsRoots, false);
            SetCollidersEnabled(stairBlockers, true);
        }

        private bool IsIntroHandoffReady()
        {
            if (introDirector == null)
            {
                return true;
            }

            double duration = introDirector.duration;
            double resolvedHandoff = introHandoffSeconds > 0d
                ? introHandoffSeconds
                : (double.IsInfinity(duration) ? 0d : duration);
            return introDirector.time >= resolvedHandoff
                || (!double.IsInfinity(duration)
                    && duration > 0d
                    && introDirector.time >= duration - 0.05d);
        }

        private void BeginIntroSwordGate()
        {
            phase = FlowPhase.IntroSwordGate;
            PrimeCombatCameraHandoff();
            StopIntroDirectorForHandoff();
            SetCamerasEnabled(introCamerasToDisable, false);
            SetAudioListenersEnabled(introAudioListenersToDisable, false);
            SetBehavioursEnabled(cutsceneBehavioursToDisableOnHandoff, false);
            SetObjectsActive(cutsceneRootsToDisableOnHandoff, false);
            SetObjectsActive(alwaysDisabledRoots, false);
            SetHudOpacity(0f);
            hudRevealTimer = -hudRevealDelaySeconds;
            SetObjectsActive(handoffRoots, true);
            SetObjectActive(player != null ? player.gameObject : null, true);
            SetSwordGateMode(true);
            SnapPlayerToHandoffGround();
            SetObjectActive(introSwordGateRoot, true);
            SetCombatHealthRootsActive(introSwordEnemies, true);
            SetBehavioursEnabled(introSwordEnemyGameplayBehaviours, true);
            SetObjectsActive(corridorCombatRoots, false);
            SetObjectsActive(corridorBoundsRoots, false);
            SetCollidersEnabled(stairBlockers, true);
            targetSelector?.ConfigureTargetCandidates(introSwordEnemies);
        }

        private void StopIntroDirectorForHandoff()
        {
            if (introDirector == null || introDirector.state != PlayState.Playing)
            {
                return;
            }

            introDirector.Stop();
        }

        private void RegisterIntroDirectorStoppedHandler()
        {
            if (introDirector != null)
            {
                introDirector.stopped -= HandleIntroDirectorStopped;
                introDirector.stopped += HandleIntroDirectorStopped;
            }
        }

        private void UnregisterIntroDirectorStoppedHandler()
        {
            if (introDirector != null)
            {
                introDirector.stopped -= HandleIntroDirectorStopped;
            }
        }

        private void HandleIntroDirectorStopped(PlayableDirector stoppedDirector)
        {
            observedIntroDirectorPlayback = true;
            HandleIntroDirectorCompleted();
        }

        private void HandleIntroDirectorCompleted()
        {
            if (phase == FlowPhase.WaitingForIntroHandoff)
            {
                BeginIntroSwordGate();
            }
        }

        private void UpdateIntroDirectorPlaybackObservation()
        {
            if (phase != FlowPhase.WaitingForIntroHandoff
                || observedIntroDirectorPlayback
                || introDirector == null)
            {
                return;
            }

            if (introDirector.state == PlayState.Playing || introDirector.time > 0.001d)
            {
                observedIntroDirectorPlayback = true;
            }
        }

        private bool HasIntroDirectorStoppedAfterObservedPlayback()
        {
            if (!observedIntroDirectorPlayback
                || introDirector == null
                || introDirector.state == PlayState.Playing)
            {
                return false;
            }

            return IsIntroHandoffReady() || introDirector.time <= 0.001d;
        }

        private void BeginWaitingForStairEntry()
        {
            phase = FlowPhase.WaitingForStairEntry;
            SetBehavioursEnabled(introSwordEnemyGameplayBehaviours, false);
            SetCollidersEnabled(stairBlockers, false);
            targetSelector?.ConfigureTargetCandidates(System.Array.Empty<CombatHealth>());
        }

        private void BeginCorridorCombat()
        {
            phase = FlowPhase.CorridorCombat;
            SetObjectsActive(alwaysDisabledRoots, false);
            SetObjectsActive(corridorCombatRoots, true);
            SetObjectsActive(corridorBoundsRoots, true);
            SetCollidersEnabled(stairBlockers, false);
            SetSwordGateMode(false);
            targetSelector?.ConfigureTargetCandidates(corridorTargets);
        }

        private void UpdateHudReveal()
        {
            if (phase == FlowPhase.WaitingForIntroHandoff)
            {
                SetHudOpacity(0f);
                return;
            }

            if (hudRevealTimer >= hudRevealDurationSeconds)
            {
                SetHudOpacity(1f);
                return;
            }

            hudRevealTimer += Time.deltaTime;
            float normalized = Mathf.Clamp01(hudRevealTimer / hudRevealDurationSeconds);
            float eased = normalized * normalized * (3f - 2f * normalized);
            SetHudOpacity(eased);
        }

        private void SetHudOpacity(float opacity)
        {
            float resolvedOpacity = Mathf.Clamp01(opacity);
            reviewHud?.SetHudOpacity(resolvedOpacity);
            mobileHud?.SetHudOpacity(resolvedOpacity);
        }

        private void SnapPlayerToHandoffGround()
        {
            if (player == null)
            {
                return;
            }

            float groundY = introSwordGateRoot != null
                ? introSwordGateRoot.transform.position.y
                : player.transform.position.y;
            if (!TryResolvePlayerFootMinY(out float footMinY))
            {
                return;
            }

            float targetMinY = groundY + 0.015f;
            if (Mathf.Abs(targetMinY - footMinY) <= 0.005f)
            {
                return;
            }

            CharacterController characterController = player.GetComponent<CharacterController>();
            bool controllerWasEnabled = characterController != null && characterController.enabled;
            if (characterController != null)
            {
                characterController.enabled = false;
            }

            Transform playerTransform = player.transform;
            Vector3 position = playerTransform.position;
            position.y += targetMinY - footMinY;
            playerTransform.position = position;

            if (characterController != null)
            {
                characterController.enabled = controllerWasEnabled;
            }
        }

        private bool TryResolvePlayerFootMinY(out float footMinY)
        {
            footMinY = float.PositiveInfinity;
            if (player == null)
            {
                return false;
            }

            SkinnedMeshRenderer[] skinnedRenderers =
                player.GetComponentsInChildren<SkinnedMeshRenderer>(includeInactive: false);
            for (int i = 0; i < skinnedRenderers.Length; i++)
            {
                SkinnedMeshRenderer renderer = skinnedRenderers[i];
                if (renderer == null || !renderer.enabled || !renderer.gameObject.activeInHierarchy)
                {
                    continue;
                }

                footMinY = Mathf.Min(footMinY, renderer.bounds.min.y);
            }

            if (!float.IsPositiveInfinity(footMinY))
            {
                return true;
            }

            CharacterController characterController = player.GetComponent<CharacterController>();
            if (characterController == null)
            {
                return false;
            }

            footMinY = characterController.bounds.min.y;
            return true;
        }

        private void SetSwordGateMode(bool swordOnly)
        {
            if (combatModeController != null)
            {
                if (swordOnly)
                {
                    combatModeController.enabled = true;
                    combatModeController.SetMeleeMode();
                    combatModeController.enabled = false;
                }
                else
                {
                    combatModeController.enabled = true;
                    combatModeController.SetRangedMode();
                }
            }

            SetBehaviourEnabled(rangedBasicAttackAction, !swordOnly);
            SetBehaviourEnabled(skill1Action, !swordOnly);
            SetBehaviourEnabled(summonSlot1Action, !swordOnly);
            for (int i = 0; i < supportSummonActions.Length; i++)
            {
                SetBehaviourEnabled(supportSummonActions[i], !swordOnly);
            }
        }

        private void PrimeCombatCameraHandoff()
        {
            if (combatCameraController == null)
            {
                return;
            }

            Camera activeIntroCamera = ResolveActiveIntroCamera();
            if (activeIntroCamera != null)
            {
                if (combatCameraHandoffPose != null)
                {
                    combatCameraController.PrimeFromHandoffPose(combatCameraHandoffPose);
                }
                else
                {
                    combatCameraController.PrimeFromHandoffCamera(activeIntroCamera);
                }

                CopyCameraPresentationSettings(activeIntroCamera);
                return;
            }

            combatCameraController.PrimeFromHandoffPose(combatCameraHandoffPose);
        }

        private void CopyCameraPresentationSettings(Camera sourceCamera)
        {
            Camera combatCamera = combatCameraController != null
                ? combatCameraController.GetComponent<Camera>()
                : null;
            if (sourceCamera == null || combatCamera == null)
            {
                return;
            }

            combatCamera.fieldOfView = sourceCamera.fieldOfView;
            combatCamera.orthographic = sourceCamera.orthographic;
            combatCamera.orthographicSize = sourceCamera.orthographicSize;
            combatCamera.clearFlags = sourceCamera.clearFlags;
            combatCamera.backgroundColor = sourceCamera.backgroundColor;
            combatCamera.allowHDR = sourceCamera.allowHDR;
            combatCamera.allowMSAA = sourceCamera.allowMSAA;
            combatCamera.nearClipPlane = sourceCamera.nearClipPlane;
            combatCamera.farClipPlane = sourceCamera.farClipPlane;

            CopyUniversalCameraData(sourceCamera, combatCamera);
        }

        private static void CopyUniversalCameraData(Camera sourceCamera, Camera targetCamera)
        {
            Component sourceData = FindComponentByTypeName(
                sourceCamera != null ? sourceCamera.gameObject : null,
                "UniversalAdditionalCameraData");
            Component targetData = FindComponentByTypeName(
                targetCamera != null ? targetCamera.gameObject : null,
                "UniversalAdditionalCameraData");
            if (sourceData == null || targetData == null)
            {
                return;
            }

            CopyPropertyValue(sourceData, targetData, "renderPostProcessing");
            CopyPropertyValue(sourceData, targetData, "antialiasing");
            CopyPropertyValue(sourceData, targetData, "antialiasingQuality");
        }

        private static Component FindComponentByTypeName(GameObject root, string typeName)
        {
            if (root == null)
            {
                return null;
            }

            Component[] components = root.GetComponents<Component>();
            for (int i = 0; i < components.Length; i++)
            {
                Component component = components[i];
                if (component != null
                    && string.Equals(component.GetType().Name, typeName, System.StringComparison.Ordinal))
                {
                    return component;
                }
            }

            return null;
        }

        private static void CopyPropertyValue(Component source, Component target, string propertyName)
        {
            const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            PropertyInfo sourceProperty = source.GetType().GetProperty(propertyName, Flags);
            PropertyInfo targetProperty = target.GetType().GetProperty(propertyName, Flags);
            if (sourceProperty == null
                || targetProperty == null
                || !sourceProperty.CanRead
                || !targetProperty.CanWrite)
            {
                return;
            }

            targetProperty.SetValue(target, sourceProperty.GetValue(source));
        }

        private Camera ResolveActiveIntroCamera()
        {
            if (introCamerasToDisable == null)
            {
                return null;
            }

            for (int i = 0; i < introCamerasToDisable.Length; i++)
            {
                Camera candidate = introCamerasToDisable[i];
                if (candidate != null && candidate.enabled && candidate.gameObject.activeInHierarchy)
                {
                    return candidate;
                }
            }

            return null;
        }

        private bool IsPlayerInsideStairTrigger()
        {
            if (player == null || stairTriggerCenter == null)
            {
                return false;
            }

            Vector3 offset = Vector3.ProjectOnPlane(
                player.transform.position - stairTriggerCenter.position,
                Vector3.up);
            return stairTriggerRadius <= 0f || offset.sqrMagnitude <= stairTriggerRadius * stairTriggerRadius;
        }

        private static int CountAlive(CombatHealth[] healths)
        {
            if (healths == null)
            {
                return 0;
            }

            int count = 0;
            for (int i = 0; i < healths.Length; i++)
            {
                if (healths[i] != null && healths[i].IsAlive)
                {
                    count++;
                }
            }

            return count;
        }

        private static void SetObjectsActive(GameObject[] objects, bool active)
        {
            if (objects == null)
            {
                return;
            }

            for (int i = 0; i < objects.Length; i++)
            {
                SetObjectActive(objects[i], active);
            }
        }

        private static void SetObjectActive(GameObject target, bool active)
        {
            if (target != null && target.activeSelf != active)
            {
                target.SetActive(active);
            }
        }

        private static void SetCollidersEnabled(Collider[] colliders, bool enabled)
        {
            if (colliders == null)
            {
                return;
            }

            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i] != null)
                {
                    colliders[i].enabled = enabled;
                }
            }
        }

        private static void SetCamerasEnabled(Camera[] cameras, bool enabled)
        {
            if (cameras == null)
            {
                return;
            }

            for (int i = 0; i < cameras.Length; i++)
            {
                if (cameras[i] != null)
                {
                    cameras[i].enabled = enabled;
                }
            }
        }

        private static void SetAudioListenersEnabled(AudioListener[] listeners, bool enabled)
        {
            if (listeners == null)
            {
                return;
            }

            for (int i = 0; i < listeners.Length; i++)
            {
                if (listeners[i] != null)
                {
                    listeners[i].enabled = enabled;
                }
            }
        }

        private static void SetCombatHealthRootsActive(CombatHealth[] healths, bool active)
        {
            if (healths == null)
            {
                return;
            }

            for (int i = 0; i < healths.Length; i++)
            {
                if (healths[i] != null)
                {
                    SetObjectActive(healths[i].gameObject, active);
                }
            }
        }

        private static void SetBehavioursEnabled(Behaviour[] behaviours, bool enabled)
        {
            if (behaviours == null)
            {
                return;
            }

            for (int i = 0; i < behaviours.Length; i++)
            {
                SetBehaviourEnabled(behaviours[i], enabled);
            }
        }

        private static void SetBehaviourEnabled(Behaviour behaviour, bool enabled)
        {
            if (behaviour != null)
            {
                behaviour.enabled = enabled;
            }
        }
    }
}
