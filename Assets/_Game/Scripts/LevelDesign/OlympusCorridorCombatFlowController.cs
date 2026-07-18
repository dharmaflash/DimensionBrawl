using System.Collections;
using DimensionBrawl.Combat;
using DimensionBrawl.Player;
using DimensionBrawl.Presentation;
using DimensionBrawl.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.Timeline;

namespace DimensionBrawl.LevelDesign
{
    [DisallowMultipleComponent]
    public sealed class OlympusCorridorCombatFlowController : MonoBehaviour
    {
        private const string CombatHudInstanceName = "PF_UI_CombatHud";
        private const float ActivePhasePollIntervalSeconds = 0.05f;

        private enum FlowPhase
        {
            WaitingForIntroHandoff,
            Tutorial,
            IntroSwordGate,
            WaitingForStairEntry,
            CorridorCombat,
            StageCleared
        }

        [Header("Canonical Stage Run")]
        [SerializeField] private PlayableStageDefinition playableStageDefinition;

        [Header("Intro Handoff")]
        [SerializeField] private PlayableDirector introDirector;
        [SerializeField, Min(0f)] private double introHandoffSeconds = 36.5d;
        [SerializeField] private bool showIntroSkipButton = true;
        [SerializeField] private Key introSkipKey = Key.Escape;
        [SerializeField] private string introSkipButtonLabel = "SKIP";
        [SerializeField] private Rect introSkipButtonNormalizedRect = new Rect(0.84f, 0.045f, 0.12f, 0.06f);
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

        [Header("Tutorial")]
        [SerializeField] private bool runTutorialAfterIntroHandoff = true;
        [SerializeField] private OlympusCorridorTutorialDirector tutorialDirector;
        [SerializeField] private AudioSource tutorialOverlayAudioSource;
        [SerializeField] private AudioClip tutorialOverlayOpenSfx;
        [SerializeField, Range(0f, 1f)] private float tutorialOverlayOpenSfxVolume = 0.82f;
        [SerializeField] private OlympusCorridorTutorialDirector.DialogueAudioCue[] tutorialOverlayDialogueAudioCues =
            OlympusCorridorTutorialDirector.CreateDefaultDialogueAudioCueSlots();

        [Header("Handoff UI Reveal")]
        [SerializeField] private CanvasGroup combatHudCanvasGroup;
        [SerializeField, Min(0f)] private float hudRevealDelaySeconds = 0.08f;
        [SerializeField, Min(0.01f)] private float hudRevealDurationSeconds = 0.18f;

        [Header("Stair To Corridor")]
        [SerializeField] private Transform stairEntryAnchor;
        [SerializeField] private Transform stairTriggerCenter;
        [SerializeField, Min(0f)] private float stairTriggerRadius = 2.75f;
        [SerializeField] private GameObject[] corridorCombatRoots = System.Array.Empty<GameObject>();
        [SerializeField] private GameObject[] corridorBoundsRoots = System.Array.Empty<GameObject>();
        [SerializeField] private CombatHealth[] corridorTargets = System.Array.Empty<CombatHealth>();
        [SerializeField] private CombatHealth[] corridorClearTargets = System.Array.Empty<CombatHealth>();

        [Header("Stage Clear")]
        [SerializeField] private OlympusStageClearOverlay stageClearOverlay;

        [Header("Combat Phase Audio")]
        [SerializeField] private AudioClip combatPhaseBgmClip;
        [SerializeField, Range(0f, 1f)] private float combatPhaseBgmVolume = 0.34f;

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
        private GUIStyle introSkipButtonStyle;
        private AudioSource runtimeTutorialOverlayAudioSource;
        private bool tutorialRouteCompletionHandled;
        private Coroutine introHandoffRoutine;
        private Coroutine hudRevealRoutine;
        private Coroutine activePhaseRoutine;
        private StageRunContext stageRunContext;
        private bool stageRunAdmissionErrorLogged;

        public bool IntroGateCleared => CountAlive(introSwordEnemies) == 0;
        public bool TutorialRunning => phase == FlowPhase.Tutorial
            && tutorialDirector != null
            && tutorialDirector.IsRunning;
        public bool TutorialCompleted => tutorialDirector != null && tutorialDirector.IsCompleted;
        public bool CorridorCleared => HasAny(corridorClearTargets) && CountAlive(corridorClearTargets) == 0;
        public bool CorridorCombatStarted => phase == FlowPhase.CorridorCombat;
        public bool StageCleared => phase == FlowPhase.StageCleared;
        public bool StageClearOverlayShown => stageClearOverlay != null && stageClearOverlay.IsShown;
        public bool HasCanonicalStageRun => stageRunContext != null
            && stageRunContext.LifecycleState != StageRunLifecycleState.Faulted
            && stageRunContext.LifecycleState != StageRunLifecycleState.Disposed;
        public string CanonicalStageRunId => stageRunContext?.Identity.RunId ?? string.Empty;

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
            Transform newStairEntryAnchor,
            Transform newStairTriggerCenter,
            float newStairTriggerRadius,
            GameObject[] newCorridorCombatRoots,
            GameObject[] newCorridorBoundsRoots,
            CombatHealth[] newCorridorTargets,
            CombatHealth[] newCorridorClearTargets,
            PlayerMovementController newPlayer,
            PlayerCombatModeController newCombatModeController,
            PlayerCombatTargetSelector newTargetSelector,
            PlayerRangedBasicAttackAction newRangedBasicAttackAction,
            PlayerSkill1Action newSkill1Action,
            PlayerSummonSlot1Action newSummonSlot1Action,
            PlayerSupportSummonSlotAction[] newSupportSummonActions,
            float newHudRevealDelaySeconds,
            float newHudRevealDurationSeconds)
        {
            UnregisterIntroDirectorStoppedHandler();
            UnregisterIntroSwordEnemyHandlers();
            UnregisterCorridorClearTargetHandlers();
            introDirector = newIntroDirector;
            if (Application.isPlaying && isActiveAndEnabled)
            {
                RegisterIntroDirectorStoppedHandler();
            }

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
            stairEntryAnchor = newStairEntryAnchor;
            stairTriggerCenter = newStairTriggerCenter;
            stairTriggerRadius = Mathf.Max(0f, newStairTriggerRadius);
            corridorCombatRoots = newCorridorCombatRoots ?? System.Array.Empty<GameObject>();
            corridorBoundsRoots = newCorridorBoundsRoots ?? System.Array.Empty<GameObject>();
            corridorTargets = newCorridorTargets ?? System.Array.Empty<CombatHealth>();
            corridorClearTargets = newCorridorClearTargets ?? System.Array.Empty<CombatHealth>();
            player = newPlayer;
            combatModeController = newCombatModeController;
            targetSelector = newTargetSelector;
            rangedBasicAttackAction = newRangedBasicAttackAction;
            skill1Action = newSkill1Action;
            summonSlot1Action = newSummonSlot1Action;
            supportSummonActions = newSupportSummonActions ?? System.Array.Empty<PlayerSupportSummonSlotAction>();
            hudRevealDelaySeconds = Mathf.Max(0f, newHudRevealDelaySeconds);
            hudRevealDurationSeconds = Mathf.Max(0.01f, newHudRevealDurationSeconds);
            ResumePhaseRoutines();
        }

        private void Awake()
        {
            ResolveStageClearOverlay();
            if (!Application.isPlaying)
            {
                return;
            }

            PrepareInitialState();
        }

        private void OnEnable()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            RegisterIntroDirectorStoppedHandler();
            PrepareInitialState();
            ResumePhaseRoutines();
        }

        private void Start()
        {
            if (Application.isPlaying)
            {
                EnsureCanonicalStageRunAdmission();
            }
        }

        private void Update()
        {
            PulseCanonicalStageRunClock();
        }

        private void OnDisable()
        {
            StopPhaseRoutines();
            SetPlayerCombatInputLocked(false);
            UnregisterIntroDirectorStoppedHandler();
            UnregisterTutorialCompletedHandler();
            UnregisterIntroSwordEnemyHandlers();
            UnregisterCorridorClearTargetHandlers();
        }

        private void ResumePhaseRoutines()
        {
            if (!Application.isPlaying || !isActiveAndEnabled)
            {
                return;
            }

            if (phase == FlowPhase.WaitingForIntroHandoff)
            {
                EnsureIntroHandoffRoutine();
                return;
            }

            EnsureHudRevealRoutine();
            if (phase != FlowPhase.StageCleared)
            {
                EnsureActivePhaseRoutine();
            }
        }

        private void StopPhaseRoutines()
        {
            StopRoutine(ref introHandoffRoutine);
            StopRoutine(ref hudRevealRoutine);
            StopRoutine(ref activePhaseRoutine);
        }

        private void StopRoutine(ref Coroutine routine)
        {
            if (routine == null)
            {
                return;
            }

            StopCoroutine(routine);
            routine = null;
        }

        private void EnsureIntroHandoffRoutine()
        {
            if (introHandoffRoutine != null || phase != FlowPhase.WaitingForIntroHandoff)
            {
                return;
            }

            introHandoffRoutine = StartCoroutine(WatchIntroHandoff());
        }

        private IEnumerator WatchIntroHandoff()
        {
            yield return null;
            while (isActiveAndEnabled && phase == FlowPhase.WaitingForIntroHandoff)
            {
                UpdateIntroDirectorPlaybackObservation();
                UpdateIntroSkipInput();
                if (phase != FlowPhase.WaitingForIntroHandoff)
                {
                    break;
                }

                if (IsIntroHandoffReady() || HasIntroDirectorStoppedAfterObservedPlayback())
                {
                    BeginPostIntroHandoffGameplay();
                    break;
                }

                yield return null;
            }

            introHandoffRoutine = null;
        }

        private void EnsureHudRevealRoutine()
        {
            if (hudRevealRoutine != null || phase == FlowPhase.WaitingForIntroHandoff)
            {
                return;
            }

            if (hudRevealTimer >= hudRevealDurationSeconds)
            {
                SetHudOpacity(1f);
                return;
            }

            hudRevealRoutine = StartCoroutine(RevealHudUntilVisible());
        }

        private IEnumerator RevealHudUntilVisible()
        {
            yield return null;
            while (isActiveAndEnabled
                && phase != FlowPhase.WaitingForIntroHandoff
                && hudRevealTimer < hudRevealDurationSeconds)
            {
                UpdateHudReveal();
                if (hudRevealTimer >= hudRevealDurationSeconds)
                {
                    break;
                }

                yield return null;
            }

            if (isActiveAndEnabled
                && phase != FlowPhase.WaitingForIntroHandoff
                && hudRevealTimer >= hudRevealDurationSeconds)
            {
                SetHudOpacity(1f);
            }

            hudRevealRoutine = null;
        }

        private void EnsureActivePhaseRoutine()
        {
            if (activePhaseRoutine != null
                || phase == FlowPhase.WaitingForIntroHandoff
                || phase == FlowPhase.StageCleared)
            {
                return;
            }

            activePhaseRoutine = StartCoroutine(ObserveActivePhaseUntilTerminal());
        }

        private IEnumerator ObserveActivePhaseUntilTerminal()
        {
            var pollDelay = new WaitForSeconds(ActivePhasePollIntervalSeconds);
            yield return null;
            while (isActiveAndEnabled
                && phase != FlowPhase.WaitingForIntroHandoff
                && phase != FlowPhase.StageCleared)
            {
                EvaluateActivePhase();
                if (phase == FlowPhase.StageCleared)
                {
                    break;
                }

                yield return pollDelay;
            }

            activePhaseRoutine = null;
        }

        private void EvaluateActivePhase()
        {
            switch (phase)
            {
                case FlowPhase.Tutorial:
                    if (tutorialDirector == null || tutorialDirector.IsCompleted)
                    {
                        CompleteTutorialAndOpenStairs();
                    }
                    break;
                case FlowPhase.IntroSwordGate:
                    TryAdvanceFromIntroSwordGate();
                    break;
                case FlowPhase.WaitingForStairEntry:
                    EnsurePlayerMovementEnabled();
                    SetPlayerLaneConstraintEnabled(false);
                    if (IsPlayerInsideStairTrigger())
                    {
                        BeginCorridorCombat();
                    }
                    break;
            }
        }

        private void OnGUI()
        {
            if (!showIntroSkipButton || !CanSkipIntroCutscene())
            {
                return;
            }

            EnsureIntroSkipButtonStyle();
            Rect rect = ResolveIntroSkipButtonRect();
            if (GUI.Button(rect, introSkipButtonLabel, introSkipButtonStyle))
            {
                SkipIntroCutscene();
            }
        }

        private void Reset()
        {
            EnsureTutorialDialogueAudioCueSlots();
            ResolveStageClearOverlay();
        }

        private void OnValidate()
        {
            EnsureTutorialDialogueAudioCueSlots();
            ResolveStageClearOverlay();
        }

        private void EnsureTutorialDialogueAudioCueSlots()
        {
            tutorialOverlayDialogueAudioCues =
                OlympusCorridorTutorialDirector.NormalizeDialogueAudioCueSlots(tutorialOverlayDialogueAudioCues);
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
            SetPlayerCombatInputLocked(true);
            SetBehavioursEnabled(introSwordEnemyGameplayBehaviours, false);
            SetObjectActive(introSwordGateRoot, false);
            SetObjectsActive(corridorCombatRoots, false);
            SetObjectsActive(corridorBoundsRoots, false);
            SetCollidersEnabled(stairBlockers, true);
            SetPlayerLaneConstraintEnabled(true);
        }

        public void SkipIntroCutscene()
        {
            if (!CanSkipIntroCutscene())
            {
                return;
            }

            observedIntroDirectorPlayback = true;
            if (introDirector != null)
            {
                introDirector.time = ResolveIntroHandoffTime();
                introDirector.Evaluate();
            }

            BeginPostIntroHandoffGameplay();
        }

        private void UpdateIntroSkipInput()
        {
            if (!CanSkipIntroCutscene() || introSkipKey == Key.None || Keyboard.current == null)
            {
                return;
            }

            var key = Keyboard.current[introSkipKey];
            if (key != null && key.wasPressedThisFrame)
            {
                SkipIntroCutscene();
            }
        }

        private bool CanSkipIntroCutscene()
        {
            return Application.isPlaying
                && phase == FlowPhase.WaitingForIntroHandoff
                && introDirector != null;
        }

        private bool IsIntroHandoffReady()
        {
            if (introDirector == null)
            {
                return true;
            }

            double duration = introDirector.duration;
            return introDirector.time >= ResolveIntroHandoffTime()
                || (!double.IsInfinity(duration)
                    && duration > 0d
                    && introDirector.time >= duration - 0.05d);
        }

        private double ResolveIntroHandoffTime()
        {
            if (introDirector == null)
            {
                return 0d;
            }

            double duration = introDirector.duration;
            double resolvedHandoff = introHandoffSeconds > 0d
                ? introHandoffSeconds
                : (double.IsInfinity(duration) ? 0d : duration);
            if (!double.IsInfinity(duration) && duration > 0d)
            {
                return System.Math.Min(resolvedHandoff, System.Math.Max(0d, duration - 0.05d));
            }

            return resolvedHandoff;
        }

        private Rect ResolveIntroSkipButtonRect()
        {
            float width = Mathf.Max(96f, introSkipButtonNormalizedRect.width * Screen.width);
            float height = Mathf.Max(40f, introSkipButtonNormalizedRect.height * Screen.height);
            float x = Mathf.Clamp01(introSkipButtonNormalizedRect.x) * Screen.width;
            float y = Mathf.Clamp01(introSkipButtonNormalizedRect.y) * Screen.height;
            return new Rect(x, y, width, height);
        }

        private void EnsureIntroSkipButtonStyle()
        {
            if (introSkipButtonStyle == null)
            {
                introSkipButtonStyle = new GUIStyle(GUI.skin.button)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontStyle = FontStyle.Bold
                };
            }

            introSkipButtonStyle.fontSize = Mathf.RoundToInt(Mathf.Clamp(Screen.height * 0.018f, 16f, 24f));
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
            SetHudOpacity(0f);
            SetObjectActive(introSwordGateRoot, true);
            SetObjectActive(player != null ? player.gameObject : null, true);
            SetPlayerCombatInputLocked(false);
            ClearPlayerInputForPhaseTransition();
            SetPlayerLaneConstraintEnabled(true);
            SetSwordGateMode(true);
            SnapPlayerToHandoffGround();
            SetCombatHealthRootsActive(introSwordEnemies, true);
            SetCombatHealthRootCollidersEnabled(introSwordEnemies, true);
            SetBehavioursEnabled(introSwordEnemyGameplayBehaviours, true);
            RegisterIntroSwordEnemyHandlers();
            SetObjectsActive(corridorCombatRoots, false);
            SetObjectsActive(corridorBoundsRoots, false);
            SetCollidersEnabled(stairBlockers, true);
            ConfigureTargetCandidates(introSwordEnemies);
            TryAdvanceFromIntroSwordGate();
            ResumePhaseRoutines();
        }

        private void BeginTutorial()
        {
            OlympusCorridorTutorialDirector director = ResolveTutorialDirector();
            if (director == null || !director.TutorialEnabled)
            {
                BeginIntroSwordGate();
                return;
            }

            phase = FlowPhase.Tutorial;
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
            SetHudOpacity(0f);
            SetObjectActive(introSwordGateRoot, true);
            SetObjectActive(player != null ? player.gameObject : null, true);
            SetPlayerCombatInputLocked(false);
            ClearPlayerInputForPhaseTransition();
            SetPlayerLaneConstraintEnabled(false);
            SetTutorialEntryMode();
            SnapPlayerToHandoffGround();
            SetCombatHealthRootsActive(introSwordEnemies, true);
            SetCombatHealthRootCollidersEnabled(introSwordEnemies, true);
            SetTutorialEnemyGameplayEnabled(introSwordEnemyGameplayBehaviours, false);
            UnregisterIntroSwordEnemyHandlers();
            SetObjectsActive(corridorCombatRoots, false);
            SetObjectsActive(corridorBoundsRoots, false);
            SetCollidersEnabled(stairBlockers, true);
            ConfigureTargetCandidates(System.Array.Empty<CombatHealth>());
            EnsureTutorialDialogueAudioCueSlots();
            director.ConfigureOverlayAudio(
                ResolveTutorialOverlayAudioSource(),
                ResolveTutorialOverlayOpenSfx(),
                tutorialOverlayOpenSfxVolume);
            director.ConfigureOverlayDialogueAudio(tutorialOverlayDialogueAudioCues);

            director.BindRuntimeContext(
                player,
                combatModeController,
                targetSelector,
                player != null ? player.GetComponent<PlayerActionController>() : null,
                rangedBasicAttackAction,
                skill1Action,
                summonSlot1Action,
                supportSummonActions,
                ResolveTutorialPromptPresenter(),
                introSwordEnemies,
                introSwordEnemyGameplayBehaviours,
                stairBlockers);
            RegisterTutorialCompletedHandler();
            director.BeginTutorial();

            if (director.IsCompleted)
            {
                CompleteTutorialAndOpenStairs();
            }

            ResumePhaseRoutines();
        }

        private void BeginPostIntroHandoffGameplay()
        {
            if (ShouldRunTutorial())
            {
                BeginTutorial();
                return;
            }

            BeginIntroSwordGate();
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
            Scene ownerScene = gameObject.scene;
            if (!Application.isPlaying
                || !isActiveAndEnabled
                || !ownerScene.IsValid()
                || !ownerScene.isLoaded
                || stoppedDirector != introDirector)
            {
                return;
            }

            observedIntroDirectorPlayback = true;
            HandleIntroDirectorCompleted();
        }

        private void HandleIntroDirectorCompleted()
        {
            if (phase == FlowPhase.WaitingForIntroHandoff)
            {
                BeginPostIntroHandoffGameplay();
                return;
            }

            if (phase == FlowPhase.IntroSwordGate)
            {
                ReassertIntroSwordGateVisibility();
            }
        }

        private void ReassertIntroSwordGateVisibility()
        {
            SetObjectsActive(handoffRoots, true);
            SetObjectActive(player != null ? player.gameObject : null, true);
            SetObjectActive(introSwordGateRoot, true);
            SetCombatHealthRootsActive(introSwordEnemies, true);
            SetCombatHealthRootCollidersEnabled(introSwordEnemies, true);
            TryAdvanceFromIntroSwordGate();
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
            UnregisterTutorialCompletedHandler();
            if (tutorialDirector != null && tutorialDirector.IsRunning)
            {
                tutorialDirector.CancelTutorial();
            }

            UnregisterIntroSwordEnemyHandlers();
            SetBehavioursEnabled(introSwordEnemyGameplayBehaviours, false);
            SetCombatHealthRootCollidersEnabled(introSwordEnemies, false);
            SetCollidersEnabled(stairBlockers, false);
            ClearPlayerInputForPhaseTransition();
            SetPlayerLaneConstraintEnabled(false);

            if (Application.isPlaying)
            {
                SceneBgmController.PlayStagePhase(combatPhaseBgmClip, combatPhaseBgmVolume);
            }
            ConfigureTargetCandidates(System.Array.Empty<CombatHealth>());
            ResumePhaseRoutines();
        }

        private void BeginCorridorCombat()
        {
            if (!EnsureCanonicalStageRunAdmission())
            {
                return;
            }

            PulseCanonicalStageRunClock();
            if (!stageRunContext.TryAdvanceCurrentSegmentInScene(
                    stageRunContext.CurrentSegment.ExitConditionId,
                    gameObject.scene,
                    out _,
                    out string advanceError))
            {
                Debug.LogError(
                    $"[{nameof(OlympusCorridorCombatFlowController)}] Canonical lower-combat in-scene advance rejected: {advanceError}",
                    this);
                return;
            }

            phase = FlowPhase.CorridorCombat;
            tutorialDirector?.HideGuide();
            UnregisterIntroSwordEnemyHandlers();
            SetCombatHealthRootCollidersEnabled(introSwordEnemies, false);
            SetObjectsActive(alwaysDisabledRoots, false);
            SetObjectsActive(corridorCombatRoots, true);
            SetObjectsActive(corridorBoundsRoots, true);
            SetCollidersEnabled(stairBlockers, false);
            ClearPlayerInputForPhaseTransition();
            SetPlayerCombatInputLocked(false);
            SetPlayerLaneConstraintEnabled(true);
            SetSwordGateMode(false);
            ConfigureTargetCandidates(corridorTargets);
            ResumePhaseRoutines();
        }

        private void BeginStageCleared()
        {
            phase = FlowPhase.StageCleared;
            UnregisterCorridorClearTargetHandlers();
            SetObjectsActive(corridorBoundsRoots, false);
            SetCollidersEnabled(stairBlockers, false);
            ClearPlayerInputForPhaseTransition();
            SetPlayerLaneConstraintEnabled(false);
            SetCombatHealthRootCollidersEnabled(corridorTargets, false);
            SetCombatHealthRootCollidersEnabled(corridorClearTargets, false);
            ConfigureTargetCandidates(System.Array.Empty<CombatHealth>());
            ResolveStageClearOverlay();
            if (stageClearOverlay != null && Application.isPlaying)
            {
                stageClearOverlay.Show();
            }
            else if (stageClearOverlay == null)
            {
                Debug.LogError($"[{nameof(OlympusCorridorCombatFlowController)}] Missing {nameof(OlympusStageClearOverlay)} on the canonical flow root.", this);
            }

            ResumePhaseRoutines();
        }

        private void ResolveStageClearOverlay()
        {
            if (stageClearOverlay == null)
            {
                stageClearOverlay = GetComponent<OlympusStageClearOverlay>();
            }
        }

        private void RegisterIntroSwordEnemyHandlers()
        {
            if (introSwordEnemies == null)
            {
                return;
            }

            for (int i = 0; i < introSwordEnemies.Length; i++)
            {
                CombatHealth health = introSwordEnemies[i];
                if (health == null)
                {
                    continue;
                }

                health.Died -= HandleIntroSwordEnemyDied;
                health.Died += HandleIntroSwordEnemyDied;
            }
        }

        private void UnregisterIntroSwordEnemyHandlers()
        {
            if (introSwordEnemies == null)
            {
                return;
            }

            for (int i = 0; i < introSwordEnemies.Length; i++)
            {
                CombatHealth health = introSwordEnemies[i];
                if (health != null)
                {
                    health.Died -= HandleIntroSwordEnemyDied;
                }
            }
        }

        private void HandleIntroSwordEnemyDied()
        {
            TryAdvanceFromIntroSwordGate();
        }

        private void RegisterCorridorClearTargetHandlers()
        {
            if (corridorClearTargets == null)
            {
                return;
            }

            for (int i = 0; i < corridorClearTargets.Length; i++)
            {
                CombatHealth health = corridorClearTargets[i];
                if (health == null)
                {
                    continue;
                }

                health.Died -= HandleCorridorClearTargetDied;
                health.Died += HandleCorridorClearTargetDied;
            }
        }

        private void UnregisterCorridorClearTargetHandlers()
        {
            if (corridorClearTargets == null)
            {
                return;
            }

            for (int i = 0; i < corridorClearTargets.Length; i++)
            {
                CombatHealth health = corridorClearTargets[i];
                if (health != null)
                {
                    health.Died -= HandleCorridorClearTargetDied;
                }
            }
        }

        private void HandleCorridorClearTargetDied()
        {
            TryAdvanceFromCorridorCombat();
        }

        private void TryAdvanceFromIntroSwordGate()
        {
            if (phase == FlowPhase.IntroSwordGate && IntroGateCleared)
            {
                BeginWaitingForStairEntry();
            }
        }

        private void HandleTutorialCompleted()
        {
            if (phase == FlowPhase.Tutorial)
            {
                CompleteTutorialAndOpenStairs();
            }
        }

        private void CompleteTutorialAndOpenStairs()
        {
            if (tutorialRouteCompletionHandled)
            {
                return;
            }

            if (!EnsureCanonicalStageRunAdmission())
            {
                return;
            }

            PulseCanonicalStageRunClock();
            OlympusCorridorTutorialDirector director = ResolveTutorialDirector();
            if (director == null || !director.IsCompleted)
            {
                Debug.LogError(
                    $"[{nameof(OlympusCorridorCombatFlowController)}] Corridor tutorial completion fact is unavailable before stair release.",
                    this);
                return;
            }

            if (stageRunContext.TutorialRouteSummaryFact == null
                && !stageRunContext.TrySealTutorialRouteCompletion(out string tutorialFactError))
            {
                Debug.LogError(
                    $"[{nameof(OlympusCorridorCombatFlowController)}] Corridor tutorial fact seal rejected: {tutorialFactError}",
                    this);
                return;
            }

            tutorialRouteCompletionHandled = true;
            UnregisterTutorialCompletedHandler();
            BeginWaitingForStairEntry();
        }

        private bool EnsureCanonicalStageRunAdmission()
        {
            if (stageRunContext != null
                && stageRunContext.CurrentSceneHandle == gameObject.scene.handle
                && stageRunContext.LifecycleState == StageRunLifecycleState.CorridorActive)
            {
                return true;
            }

            string admissionError = "PlayableStageDefinition is missing.";
            if (playableStageDefinition != null
                && StageRunRuntime.TryAdmitFirstSegment(
                    playableStageDefinition,
                    gameObject.scene,
                    out stageRunContext,
                    out admissionError))
            {
                stageRunAdmissionErrorLogged = false;
                return true;
            }

            if (!stageRunAdmissionErrorLogged)
            {
                stageRunAdmissionErrorLogged = true;
                Debug.LogError(
                    $"[{nameof(OlympusCorridorCombatFlowController)}] Canonical run admission failed: "
                    + admissionError,
                    this);
            }

            return false;
        }

        private void PulseCanonicalStageRunClock()
        {
            if (!Application.isPlaying
                || stageRunContext == null
                || stageRunContext.LifecycleState != StageRunLifecycleState.CorridorActive)
            {
                return;
            }

            if (!stageRunContext.TryPulseActiveTime(
                    Time.realtimeSinceStartupAsDouble,
                    Application.isBatchMode || Application.isFocused,
                    false,
                    false,
                    false,
                    out string clockError))
            {
                Debug.LogError(
                    $"[{nameof(OlympusCorridorCombatFlowController)}] Canonical run clock rejected: {clockError}",
                    this);
            }
        }

        private bool ShouldRunTutorial()
        {
            OlympusCorridorTutorialDirector director = ResolveTutorialDirector();
            return runTutorialAfterIntroHandoff
                && director != null
                && director.TutorialEnabled;
        }

        private OlympusCorridorTutorialDirector ResolveTutorialDirector()
        {
            if (tutorialDirector == null)
            {
                tutorialDirector = GetComponent<OlympusCorridorTutorialDirector>();
            }

            if (tutorialDirector == null && runTutorialAfterIntroHandoff && Application.isPlaying)
            {
                tutorialDirector = gameObject.AddComponent<OlympusCorridorTutorialDirector>();
            }

            return tutorialDirector;
        }

        private CinematicTutorialPromptPresenter ResolveTutorialPromptPresenter()
        {
            CinematicTutorialPromptPresenter[] presenters =
                Object.FindObjectsByType<CinematicTutorialPromptPresenter>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None);
            return presenters.Length > 0 ? presenters[0] : null;
        }

        private AudioSource ResolveTutorialOverlayAudioSource()
        {
            if (tutorialOverlayAudioSource != null)
            {
                return tutorialOverlayAudioSource;
            }

            if (runtimeTutorialOverlayAudioSource != null)
            {
                return runtimeTutorialOverlayAudioSource;
            }

            if (!Application.isPlaying)
            {
                return null;
            }

            runtimeTutorialOverlayAudioSource = gameObject.AddComponent<AudioSource>();
            runtimeTutorialOverlayAudioSource.playOnAwake = false;
            runtimeTutorialOverlayAudioSource.loop = false;
            runtimeTutorialOverlayAudioSource.spatialBlend = 0f;
            return runtimeTutorialOverlayAudioSource;
        }

        private AudioClip ResolveTutorialOverlayOpenSfx()
        {
            if (tutorialOverlayOpenSfx != null)
            {
                return tutorialOverlayOpenSfx;
            }

            TimelineAsset timeline = introDirector != null ? introDirector.playableAsset as TimelineAsset : null;
            if (timeline == null)
            {
                return null;
            }

            AudioClip earliestVoiceClip = null;
            double earliestVoiceStart = double.MaxValue;
            foreach (TrackAsset track in timeline.GetOutputTracks())
            {
                if (track == null)
                {
                    continue;
                }

                bool isVoiceTrack = IsVoiceTimelineName(track.name);
                foreach (TimelineClip clip in track.GetClips())
                {
                    AudioPlayableAsset audioPlayable = clip.asset as AudioPlayableAsset;
                    if (audioPlayable == null || audioPlayable.clip == null)
                    {
                        continue;
                    }

                    if (!isVoiceTrack && !IsVoiceTimelineName(clip.displayName))
                    {
                        continue;
                    }

                    if (clip.start < earliestVoiceStart)
                    {
                        earliestVoiceClip = audioPlayable.clip;
                        earliestVoiceStart = clip.start;
                    }
                }
            }

            return earliestVoiceClip;
        }

        private static bool IsVoiceTimelineName(string value)
        {
            return !string.IsNullOrWhiteSpace(value)
                && value.IndexOf("Voice", System.StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void RegisterTutorialCompletedHandler()
        {
            if (tutorialDirector == null)
            {
                return;
            }

            tutorialDirector.Completed -= HandleTutorialCompleted;
            tutorialDirector.Completed += HandleTutorialCompleted;
        }

        private void UnregisterTutorialCompletedHandler()
        {
            if (tutorialDirector != null)
            {
                tutorialDirector.Completed -= HandleTutorialCompleted;
            }
        }

        private void TryAdvanceFromCorridorCombat()
        {
            if (phase == FlowPhase.CorridorCombat && CorridorCleared)
            {
                BeginStageCleared();
            }
        }

        private void ConfigureTargetCandidates(CombatHealth[] candidates)
        {
            if (targetSelector != null)
            {
                targetSelector.ConfigureTargetCandidates(candidates);
            }
        }

        private void SetPlayerLaneConstraintEnabled(bool enabled)
        {
            if (player != null)
            {
                player.SetLaneConstraintEnabled(enabled);
            }
        }

        private void ClearPlayerInputForPhaseTransition()
        {
            EnsurePlayerMovementEnabled();
            if (player == null)
            {
                return;
            }

            player.SetMoveInput(Vector2.zero);
            player.SetLookInput(Vector2.zero);
        }

        private void EnsurePlayerMovementEnabled()
        {
            if (player == null)
            {
                return;
            }

            player.enabled = true;
            CharacterController characterController = player.GetComponent<CharacterController>();
            if (characterController != null)
            {
                characterController.enabled = true;
            }
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
            SetCombatHudCanvasGroupOpacity(Mathf.Clamp01(opacity));
        }

        private void SetCombatHudCanvasGroupOpacity(float opacity)
        {
            CanvasGroup canvasGroup = ResolveCombatHudCanvasGroup();
            if (canvasGroup == null)
            {
                return;
            }

            bool acceptsInput = opacity > 0.999f;
            canvasGroup.alpha = opacity;
            canvasGroup.interactable = acceptsInput;
            canvasGroup.blocksRaycasts = acceptsInput;
        }

        private CanvasGroup ResolveCombatHudCanvasGroup()
        {
            if (combatHudCanvasGroup != null)
            {
                return combatHudCanvasGroup;
            }

            if (handoffRoots == null)
            {
                return null;
            }

            for (int i = 0; i < handoffRoots.Length; i++)
            {
                GameObject root = handoffRoots[i];
                if (root == null)
                {
                    continue;
                }

                CanvasGroup canvasGroup = FindNamedCanvasGroup(root.transform, CombatHudInstanceName);
                if (canvasGroup == null)
                {
                    continue;
                }

                combatHudCanvasGroup = canvasGroup;
                return combatHudCanvasGroup;
            }

            return null;
        }

        private static CanvasGroup FindNamedCanvasGroup(Transform root, string objectName)
        {
            if (root == null || string.IsNullOrWhiteSpace(objectName))
            {
                return null;
            }

            if (string.Equals(root.name, objectName, System.StringComparison.Ordinal))
            {
                return root.GetComponent<CanvasGroup>();
            }

            for (int i = 0; i < root.childCount; i++)
            {
                CanvasGroup canvasGroup = FindNamedCanvasGroup(root.GetChild(i), objectName);
                if (canvasGroup != null)
                {
                    return canvasGroup;
                }
            }

            return null;
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
                combatModeController.SetCinematicInputLocked(
                    PlayerInputLockSource.CorridorCombatFlow,
                    false);
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

        private void SetTutorialEntryMode()
        {
            if (combatModeController != null)
            {
                combatModeController.enabled = true;
                combatModeController.SetCinematicInputLocked(
                    PlayerInputLockSource.CorridorCombatFlow,
                    false);
                combatModeController.SetMeleeMode();
            }

            SetBehaviourEnabled(rangedBasicAttackAction, true);
            SetBehaviourEnabled(skill1Action, true);
            SetBehaviourEnabled(summonSlot1Action, true);
            for (int i = 0; i < supportSummonActions.Length; i++)
            {
                SetBehaviourEnabled(supportSummonActions[i], true);
            }
        }

        private void SetPlayerCombatInputLocked(bool locked)
        {
            if (player != null && player.TryGetComponent(out PlayerActionController actionController))
            {
                actionController.SetCinematicInputLocked(PlayerInputLockSource.CorridorCombatFlow, locked);
            }

            combatModeController?.SetCinematicInputLocked(PlayerInputLockSource.CorridorCombatFlow, locked);
            rangedBasicAttackAction?.SetCinematicInputLocked(PlayerInputLockSource.CorridorCombatFlow, locked);
            skill1Action?.SetCinematicInputLocked(PlayerInputLockSource.CorridorCombatFlow, locked);
            summonSlot1Action?.SetCinematicInputLocked(PlayerInputLockSource.CorridorCombatFlow, locked);
            if (supportSummonActions == null)
            {
                return;
            }

            for (int i = 0; i < supportSummonActions.Length; i++)
            {
                supportSummonActions[i]?.SetCinematicInputLocked(
                    PlayerInputLockSource.CorridorCombatFlow,
                    locked);
            }
        }

        private void PrimeCombatCameraHandoff()
        {
            if (combatCameraController == null)
            {
                return;
            }

            combatCameraController.CaptureBaseFieldOfViewFromControlledCamera();
            Camera activeIntroCamera = ResolveActiveIntroCamera();
            if (activeIntroCamera != null)
            {
                CopyCameraPresentation(
                    activeIntroCamera,
                    combatCameraController.GetComponent<Camera>());
                if (combatCameraHandoffPose != null)
                {
                    combatCameraController.PrimeFromHandoffPose(combatCameraHandoffPose);
                }
                else
                {
                    combatCameraController.PrimeFromHandoffCamera(activeIntroCamera);
                }

                return;
            }

            combatCameraController.PrimeFromHandoffPose(combatCameraHandoffPose);
        }

        private static void CopyCameraPresentation(Camera source, Camera target)
        {
            if (source == null || target == null)
            {
                return;
            }

            target.fieldOfView = source.fieldOfView;
            target.clearFlags = source.clearFlags;
            target.backgroundColor = source.backgroundColor;
            target.allowHDR = source.allowHDR;
            target.allowMSAA = source.allowMSAA;
            target.nearClipPlane = source.nearClipPlane;
            target.farClipPlane = source.farClipPlane;

            UniversalAdditionalCameraData sourceData =
                source.GetComponent<UniversalAdditionalCameraData>();
            UniversalAdditionalCameraData targetData =
                target.GetComponent<UniversalAdditionalCameraData>();
            if (sourceData == null || targetData == null)
            {
                return;
            }

            targetData.renderPostProcessing = sourceData.renderPostProcessing;
            targetData.antialiasing = sourceData.antialiasing;
            targetData.antialiasingQuality = sourceData.antialiasingQuality;
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

            Vector3 offset = player.transform.position - stairTriggerCenter.position;
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
                if (healths[i] != null
                    && healths[i].gameObject.activeInHierarchy
                    && healths[i].IsAlive)
                {
                    count++;
                }
            }

            return count;
        }

        private static bool HasAny(CombatHealth[] healths)
        {
            if (healths == null)
            {
                return false;
            }

            for (int i = 0; i < healths.Length; i++)
            {
                if (healths[i] != null)
                {
                    return true;
                }
            }

            return false;
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

        private static void SetCombatHealthRootCollidersEnabled(CombatHealth[] healths, bool enabled)
        {
            if (healths == null)
            {
                return;
            }

            for (int i = 0; i < healths.Length; i++)
            {
                CombatHealth health = healths[i];
                if (health == null)
                {
                    continue;
                }

                Collider[] colliders = health.GetComponentsInChildren<Collider>(includeInactive: true);
                for (int colliderIndex = 0; colliderIndex < colliders.Length; colliderIndex++)
                {
                    if (colliders[colliderIndex] != null)
                    {
                        colliders[colliderIndex].enabled = enabled;
                    }
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

        private static void SetTutorialEnemyGameplayEnabled(Behaviour[] behaviours, bool enabled)
        {
            if (behaviours == null)
            {
                return;
            }

            for (int i = 0; i < behaviours.Length; i++)
            {
                Behaviour behaviour = behaviours[i];
                if (behaviour == null)
                {
                    continue;
                }

                // Passive tutorial targets must stay resolvable and damageable while their
                // AI, sensors, and presentation behaviours are intentionally quiesced.
                behaviour.enabled = behaviour is CombatHealth || enabled;
            }
        }
    }
}
