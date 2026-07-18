using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using DimensionBrawl.LevelDesign;
using DimensionBrawl.UI;
using DimensionBrawl.UI.ContentFactoryReview;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DimensionBrawl.Editor.ContentFactoryReview
{
    /// <summary>
    /// Deterministic Play-mode visual QA for the isolated CF-01 inspection board.
    ///
    /// Batch invocation must omit Unity's -quit argument. This runner owns the
    /// asynchronous Play-mode lifecycle and exits only after 21 exact-resolution
    /// captures plus Edit-mode setup/protected-asset postflight verification:
    /// -executeMethod DimensionBrawl.Editor.ContentFactoryReview.ContentFactoryEncounterPlanReviewVisualQaCapture.RunBatchCaptureAndVerify
    /// </summary>
    [InitializeOnLoad]
    public static class ContentFactoryEncounterPlanReviewVisualQaCapture
    {
        public const string ScenePath =
            ContentFactoryEncounterPlanReviewSetup.ScenePath;
        public const string OutputDirectory =
            "C:/tmp/DimensionBrawl-ContentFactoryEncounterPlanReview-QA";

        private const string ManifestPath = OutputDirectory + "/capture-manifest.json";
        private const string ReportPath = OutputDirectory + "/capture-report.md";
        private const string ResponsiveCatalogPath =
            "Assets/_Game/DesignData/UI/DB_UIResponsiveLayouts.asset";
        private const string ExpectedResponsiveLayoutId = "AndroidLandscape";
        private const string SessionPrefix =
            "DimensionBrawl.ContentFactoryEncounterPlanReview.VisualQa.";
        private const string ActiveKey = SessionPrefix + "Active";
        private const string BatchExitKey = SessionPrefix + "BatchExit";
        private const string PhaseKey = SessionPrefix + "Phase";
        private const string FailureKey = SessionPrefix + "Failure";
        private const string StartedUtcTicksKey = SessionPrefix + "StartedUtcTicks";
        private const string SetupBeforeKey = SessionPrefix + "SetupBefore";
        private const string ProtectedDigestBeforeKey =
            SessionPrefix + "ProtectedDigestBefore";
        private const int InitialWarmupFrames = 8;
        private const int StateSettleFrames = 4;
        private const int ExpectedCaptureCount = 21;
        private const int ExpectedWaveCount = 3;
        private const int ExpectedActionButtonCount = 5;
        private const float MinimumTouchTargetPixels = 48f;
        private const float ExpectedReferenceWidth = 2400f;
        private const float ExpectedReferenceHeight = 1080f;
        private const float ExpectedMatchWidthOrHeight = 0.5f;
        private const float ExpectedEdgeInsetPixels = 32f;
        private const float LayoutFloatTolerance = 0.001f;
        private const double LaunchTimeoutSeconds = 180d;

        private enum RunnerPhase
        {
            None = 0,
            RequestedPlayMode = 1,
            Capturing = 2,
            SuccessAwaitingEditMode = 3,
            FailureAwaitingEditMode = 4
        }

        private enum ReviewCaptureState
        {
            Ready = 0,
            Wave1Active = 1,
            Wave1Partial = 2,
            Wave1Transition = 3,
            Wave2Active = 4,
            Interrupted = 5,
            Completed = 6
        }

        private static readonly CapturePlan[] Plans = BuildCapturePlans();
        private static readonly List<CaptureRecord> Records =
            new List<CaptureRecord>(ExpectedCaptureCount);

        private static ContentFactoryEncounterPlanReviewController controller;
        private static StageEncounterPlanProfile profile;
        private static Camera reviewCamera;
        private static Canvas reviewCanvas;
        private static CanvasScaler reviewCanvasScaler;
        private static UIResponsiveRoot responsiveRoot;
        private static UISafeAreaRoot safeAreaRoot;
        private static UIResponsiveLayoutCatalog responsiveCatalog;
        private static Button[] actionButtons = Array.Empty<Button>();
        private static int planIndex;
        private static int readyAtFrame;
        private static bool statePrepared;
        private static bool runtimeInitialized;

        static ContentFactoryEncounterPlanReviewVisualQaCapture()
        {
            EditorApplication.update -= Tick;
            EditorApplication.update += Tick;
            EditorApplication.playModeStateChanged -= HandlePlayModeStateChanged;
            EditorApplication.playModeStateChanged += HandlePlayModeStateChanged;
        }

        [MenuItem("Tools/DimensionBrawl/Review/Capture Content Factory Encounter Plan Visual QA")]
        public static void CaptureFromMenu()
        {
            StartCapture(exitEditorWhenFinished: false);
        }

        public static void RunBatchCaptureAndVerify()
        {
            StartCapture(exitEditorWhenFinished: true);
        }

        public static void RunBatchCapture()
        {
            RunBatchCaptureAndVerify();
        }

        private static void StartCapture(bool exitEditorWhenFinished)
        {
            try
            {
                if (SessionState.GetBool(ActiveKey, false))
                {
                    throw new InvalidOperationException(
                        "A CF-01 visual QA capture is already active.");
                }

                if (EditorApplication.isPlayingOrWillChangePlaymode)
                {
                    throw new InvalidOperationException(
                        "CF-01 visual QA must start from Edit mode.");
                }

                if (!exitEditorWhenFinished
                    && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    return;
                }

                ResetOutputArtifacts();
                ResetRuntimeFields();
                ClearSessionState();
                SessionState.SetBool(BatchExitKey, exitEditorWhenFinished);
                SessionState.SetBool(SetupBeforeKey, false);
                SessionState.SetString(ProtectedDigestBeforeKey, string.Empty);

                ContentFactoryEncounterPlanReviewSetup.RunBatchVerification();
                RequireGeneratedInput(ScenePath, "review scene");
                RequireGeneratedInput(
                    ContentFactoryEncounterPlanReviewSetup.ProfilePath,
                    "review profile");

                string digestBefore =
                    ContentFactoryEncounterPlanReviewSetup.ComputeProtectedAssetDigest();
                if (string.IsNullOrWhiteSpace(digestBefore))
                {
                    throw new InvalidOperationException(
                        "CF-01 protected-asset digest was empty before capture.");
                }

                SessionState.SetBool(SetupBeforeKey, true);
                SessionState.SetString(ProtectedDigestBeforeKey, digestBefore);
                SessionState.SetBool(ActiveKey, true);
                SessionState.SetInt(PhaseKey, (int)RunnerPhase.RequestedPlayMode);
                SessionState.SetString(FailureKey, string.Empty);
                SessionState.SetString(
                    StartedUtcTicksKey,
                    DateTime.UtcNow.Ticks.ToString(CultureInfo.InvariantCulture));

                EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                Debug.Log(
                    "[ContentFactoryEncounterPlanReviewVisualQA] Entering Play mode for "
                    + $"{ExpectedCaptureCount} exact-resolution captures.");
                EditorApplication.EnterPlaymode();
            }
            catch (Exception exception)
            {
                HandleLaunchFailure(exception, exitEditorWhenFinished);
            }
        }

        private static void HandlePlayModeStateChanged(PlayModeStateChange state)
        {
            if (!SessionState.GetBool(ActiveKey, false))
            {
                return;
            }

            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                SessionState.SetInt(PhaseKey, (int)RunnerPhase.Capturing);
                ResetRuntimeFields();
                readyAtFrame = Time.frameCount + InitialWarmupFrames;
            }
        }

        private static void Tick()
        {
            if (!SessionState.GetBool(ActiveKey, false))
            {
                return;
            }

            RunnerPhase phase = (RunnerPhase)SessionState.GetInt(
                PhaseKey,
                (int)RunnerPhase.None);
            if ((phase == RunnerPhase.SuccessAwaitingEditMode
                    || phase == RunnerPhase.FailureAwaitingEditMode)
                && !EditorApplication.isPlayingOrWillChangePlaymode)
            {
                FinalizeEditorSession(phase == RunnerPhase.SuccessAwaitingEditMode);
                return;
            }

            if (HasTimedOut())
            {
                FinishWithFailure(
                    $"CF-01 visual QA exceeded {LaunchTimeoutSeconds:0} seconds.");
                return;
            }

            if (phase == RunnerPhase.RequestedPlayMode)
            {
                if (EditorApplication.isPlaying)
                {
                    SessionState.SetInt(PhaseKey, (int)RunnerPhase.Capturing);
                    ResetRuntimeFields();
                    readyAtFrame = Time.frameCount + InitialWarmupFrames;
                }

                return;
            }

            if (phase != RunnerPhase.Capturing
                || !EditorApplication.isPlaying
                || EditorApplication.isPaused
                || EditorApplication.isCompiling)
            {
                return;
            }

            try
            {
                TickCaptureInPlayMode();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                FinishWithFailure(exception.ToString());
            }
        }

        private static void TickCaptureInPlayMode()
        {
            if (!runtimeInitialized)
            {
                ResolveRuntimeBindings();
                runtimeInitialized = true;
                readyAtFrame = Time.frameCount + InitialWarmupFrames;
                return;
            }

            if (Time.frameCount < readyAtFrame)
            {
                return;
            }

            if (planIndex >= Plans.Length)
            {
                CompleteCaptureSet();
                return;
            }

            CapturePlan plan = Plans[planIndex];
            if (!statePrepared)
            {
                PrepareState(plan.State);
                statePrepared = true;
                readyAtFrame = Time.frameCount + StateSettleFrames;
                return;
            }

            ValidateExpectedRuntimeState(plan.State);
            CaptureRecord record = CapturePlanFrame(plan);
            Records.Add(record);
            Debug.Log(
                "[ContentFactoryEncounterPlanReviewVisualQA] CAPTURE_PASS "
                + $"{record.State} {record.Width}x{record.Height} `{record.Path}`");

            planIndex++;
            statePrepared = false;
            readyAtFrame = Time.frameCount + 1;
        }

        private static void ResolveRuntimeBindings()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            if (!activeScene.IsValid()
                || !string.Equals(activeScene.path, ScenePath, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"Active Play-mode scene is `{activeScene.path}`, expected `{ScenePath}`.");
            }

            controller =
                FindSingleInScene<ContentFactoryEncounterPlanReviewController>(activeScene);
            reviewCamera = FindSingleInScene<Camera>(activeScene);
            reviewCanvas = FindSingleInScene<Canvas>(activeScene);
            reviewCanvasScaler = reviewCanvas.GetComponent<CanvasScaler>()
                ?? throw new InvalidOperationException(
                    "CF-01 review Canvas is missing its CanvasScaler.");
            responsiveRoot = FindSingleInScene<UIResponsiveRoot>(activeScene);
            safeAreaRoot = FindSingleInScene<UISafeAreaRoot>(activeScene);
            actionButtons = FindAllInScene<Button>(activeScene);
            profile = AssetDatabase.LoadAssetAtPath<StageEncounterPlanProfile>(
                ContentFactoryEncounterPlanReviewSetup.ProfilePath);
            responsiveCatalog = AssetDatabase.LoadAssetAtPath<UIResponsiveLayoutCatalog>(
                ResponsiveCatalogPath);

            if (profile == null)
            {
                throw new InvalidOperationException(
                    "CF-01 exact review profile could not be loaded at `"
                    + ContentFactoryEncounterPlanReviewSetup.ProfilePath + "`.");
            }

            if (!profile.TryValidate(out string profileError))
            {
                throw new InvalidOperationException(
                    "CF-01 exact review profile is invalid: " + profileError);
            }

            if (profile.WaveCount != ExpectedWaveCount)
            {
                throw new InvalidOperationException(
                    $"CF-01 profile must expose exactly {ExpectedWaveCount} waves; "
                    + $"found {profile.WaveCount}.");
            }

            if (!controller.ReloadProfile()
                || controller.Session == null
                || !string.IsNullOrEmpty(controller.ProfileValidationError))
            {
                throw new InvalidOperationException(
                    "CF-01 controller could not construct the exact review session: "
                    + controller.ProfileValidationError);
            }

            if (reviewCanvasScaler.uiScaleMode
                    != CanvasScaler.ScaleMode.ScaleWithScreenSize
                || reviewCanvasScaler.screenMatchMode
                    != CanvasScaler.ScreenMatchMode.MatchWidthOrHeight)
            {
                throw new InvalidOperationException(
                    "CF-01 CanvasScaler must use ScaleWithScreenSize and "
                    + "MatchWidthOrHeight.");
            }

            ValidateResponsiveBindings();
            ValidateControllerProfileAndArrays();
            ValidateActionButtonDefinitions();
            ValidateProfileProjection();
            EnsureNoActiveStageRun("runtime binding");
        }

        private static void ValidateResponsiveBindings()
        {
            if (responsiveCatalog == null)
            {
                throw new InvalidOperationException(
                    "CF-01 responsive catalog could not be loaded at `"
                    + ResponsiveCatalogPath + "`.");
            }

            var serializedRoot = new SerializedObject(responsiveRoot);
            serializedRoot.UpdateIfRequiredOrScript();
            SerializedProperty catalogProperty = serializedRoot.FindProperty("catalog")
                ?? throw new InvalidOperationException(
                    "CF-01 UIResponsiveRoot catalog field is unavailable.");
            SerializedProperty scalerProperty = serializedRoot.FindProperty("canvasScaler")
                ?? throw new InvalidOperationException(
                    "CF-01 UIResponsiveRoot CanvasScaler field is unavailable.");
            SerializedProperty safeAreaProperty = serializedRoot.FindProperty("safeAreaRoot")
                ?? throw new InvalidOperationException(
                    "CF-01 UIResponsiveRoot safe-area field is unavailable.");
            SerializedProperty applyScalerProperty =
                serializedRoot.FindProperty("applyCanvasScaler")
                ?? throw new InvalidOperationException(
                    "CF-01 UIResponsiveRoot applyCanvasScaler field is unavailable.");

            if (catalogProperty.objectReferenceValue != responsiveCatalog
                || scalerProperty.objectReferenceValue != reviewCanvasScaler
                || safeAreaProperty.objectReferenceValue != safeAreaRoot
                || !applyScalerProperty.boolValue)
            {
                throw new InvalidOperationException(
                    "CF-01 UIResponsiveRoot is not bound to the exact catalog, scaler, "
                    + "safe-area root, and runtime scaler policy.");
            }

            foreach (CapturePlan plan in Plans)
            {
                ResolveAndValidateResponsiveLayout(plan);
            }
        }

        private static void ValidateControllerProfileAndArrays()
        {
            if (!controller.HasExactWaveCardArrays)
            {
                throw new InvalidOperationException(
                    "CF-01 controller does not expose exact three-wave card arrays.");
            }

            var serializedController = new SerializedObject(controller);
            serializedController.UpdateIfRequiredOrScript();
            SerializedProperty profileProperty = serializedController.FindProperty("profile")
                ?? throw new InvalidOperationException(
                    "CF-01 controller serialized profile field is unavailable.");
            if (profileProperty.objectReferenceValue != profile)
            {
                throw new InvalidOperationException(
                    "CF-01 controller is not bound to the exact generated review profile.");
            }

            ValidateExactObjectArray(
                serializedController,
                "waveTitleTexts",
                ExpectedWaveCount);
            ValidateExactObjectArray(
                serializedController,
                "waveStateTexts",
                ExpectedWaveCount);
            ValidateExactObjectArray(
                serializedController,
                "waveDetailTexts",
                ExpectedWaveCount);
            ValidateExactObjectArray(
                serializedController,
                "waveAccentImages",
                ExpectedWaveCount);
        }

        private static void ValidateExactObjectArray(
            SerializedObject owner,
            string propertyName,
            int expectedCount)
        {
            SerializedProperty property = owner.FindProperty(propertyName)
                ?? throw new InvalidOperationException(
                    $"CF-01 controller array `{propertyName}` is unavailable.");
            if (!property.isArray || property.arraySize != expectedCount)
            {
                throw new InvalidOperationException(
                    $"CF-01 controller array `{propertyName}` must contain exactly "
                    + $"{expectedCount} entries; found {property.arraySize}.");
            }

            var uniqueReferences = new HashSet<UnityEngine.Object>();
            for (int index = 0; index < expectedCount; index++)
            {
                UnityEngine.Object value =
                    property.GetArrayElementAtIndex(index).objectReferenceValue;
                if (value == null)
                {
                    throw new InvalidOperationException(
                        $"CF-01 controller array `{propertyName}` entry {index} is null.");
                }

                if (!uniqueReferences.Add(value))
                {
                    throw new InvalidOperationException(
                        $"CF-01 controller array `{propertyName}` reuses `{value.name}`.");
                }
            }
        }

        private static void ValidateActionButtonDefinitions()
        {
            if (actionButtons.Length != ExpectedActionButtonCount)
            {
                throw new InvalidOperationException(
                    $"CF-01 scene must contain exactly {ExpectedActionButtonCount} action "
                    + $"buttons; found {actionButtons.Length}.");
            }

            var discoveredButtons = new HashSet<Button>(actionButtons);
            if (discoveredButtons.Count != ExpectedActionButtonCount)
            {
                throw new InvalidOperationException(
                    "CF-01 scene action-button discovery contains duplicate references.");
            }

            var serializedController = new SerializedObject(controller);
            serializedController.UpdateIfRequiredOrScript();
            string[] controllerFields =
            {
                "beginButton",
                "resolveButton",
                "advanceButton",
                "interruptButton",
                "resetButton"
            };
            string[] expectedObjectNames =
            {
                "BeginButton",
                "ResolveButton",
                "AdvanceButton",
                "InterruptButton",
                "ResetButton"
            };
            var mappedButtons = new HashSet<Button>();
            for (int index = 0; index < controllerFields.Length; index++)
            {
                SerializedProperty property =
                    serializedController.FindProperty(controllerFields[index])
                    ?? throw new InvalidOperationException(
                        $"CF-01 controller action field `{controllerFields[index]}` "
                        + "is unavailable.");
                Button mapped = property.objectReferenceValue as Button;
                if (mapped == null
                    || !discoveredButtons.Contains(mapped)
                    || !mappedButtons.Add(mapped)
                    || !string.Equals(
                        mapped.gameObject.name,
                        expectedObjectNames[index],
                        StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        $"CF-01 controller action field `{controllerFields[index]}` must map "
                        + $"uniquely to scene button `{expectedObjectNames[index]}`.");
                }
            }

            if (!mappedButtons.SetEquals(discoveredButtons))
            {
                throw new InvalidOperationException(
                    "CF-01 controller action fields do not exactly cover the five scene buttons.");
            }

            var names = new HashSet<string>(StringComparer.Ordinal);
            foreach (Button button in actionButtons)
            {
                if (button == null || button.onClick == null)
                {
                    throw new InvalidOperationException(
                        "CF-01 contains a null action button or onClick event.");
                }

                if (!names.Add(button.gameObject.name))
                {
                    throw new InvalidOperationException(
                        $"CF-01 action button name `{button.gameObject.name}` is duplicated.");
                }

                if (button.onClick.GetPersistentEventCount() != 0)
                {
                    throw new InvalidOperationException(
                        $"CF-01 button `{button.gameObject.name}` must not contain "
                        + "persistent UnityEvent callbacks.");
                }

                if (button.transform is not RectTransform)
                {
                    throw new InvalidOperationException(
                        $"CF-01 button `{button.gameObject.name}` has no RectTransform.");
                }
            }
        }

        private static void ValidateProfileProjection()
        {
            StageEncounterPlanReviewSession session = controller.Session
                ?? throw new InvalidOperationException(
                    "CF-01 controller session is unavailable.");
            if (session.WaveCount != ExpectedWaveCount
                || session.SchemaVersion != profile.SchemaVersion
                || session.Revision != profile.Revision
                || !string.Equals(session.PlanId, profile.PlanId, StringComparison.Ordinal)
                || !string.Equals(session.StageId, profile.StageId, StringComparison.Ordinal)
                || !string.Equals(
                    session.EncounterId,
                    profile.EncounterId,
                    StringComparison.Ordinal)
                || !string.Equals(
                    session.CanonicalDigest,
                    profile.CanonicalDigest,
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "CF-01 controller session is not an exact projection of the generated profile.");
            }

            for (int waveIndex = 0; waveIndex < ExpectedWaveCount; waveIndex++)
            {
                StageEncounterPlanProfile.WaveDefinition expected =
                    profile.GetWave(waveIndex);
                StageEncounterPlanProfile.WaveDefinition actual =
                    session.GetWave(waveIndex);
                if (expected == null
                    || actual == null
                    || expected.WaveIndex != waveIndex
                    || actual.WaveIndex != expected.WaveIndex
                    || !string.Equals(
                        actual.WaveId,
                        expected.WaveId,
                        StringComparison.Ordinal)
                    || actual.Activation != expected.Activation
                    || actual.Objective != StageEncounterObjective.DefeatAll
                    || actual.Objective != expected.Objective
                    || actual.SpawnCount != expected.SpawnCount
                    || actual.TotalCombatantCount != expected.TotalCombatantCount)
                {
                    throw new InvalidOperationException(
                        $"CF-01 session wave {waveIndex} does not exactly project the profile.");
                }
            }
        }

        private static void PrepareState(ReviewCaptureState state)
        {
            // Every plan begins at the public reset boundary. ReloadProfile then creates a
            // fresh deterministic session so completion/interruption counters cannot leak
            // from an earlier state or resolution.
            controller.ResetReview();
            if (!controller.ReloadProfile())
            {
                throw new InvalidOperationException(
                    $"CF-01 ReloadProfile failed while preparing {state}: "
                    + controller.ProfileValidationError);
            }

            ValidateFreshSession();
            switch (state)
            {
                case ReviewCaptureState.Ready:
                    break;

                case ReviewCaptureState.Wave1Active:
                    RequireAction(controller.BeginEncounter(), "BeginEncounter", state);
                    break;

                case ReviewCaptureState.Wave1Partial:
                    RequireAction(controller.BeginEncounter(), "BeginEncounter", state);
                    if (controller.Session.CurrentRemainingCombatantCount < 2)
                    {
                        throw new InvalidOperationException(
                            "Wave1Partial requires at least two Wave 1 combatants.");
                    }

                    ResolveOneNextSpawn(state);
                    break;

                case ReviewCaptureState.Wave1Transition:
                    RequireAction(controller.BeginEncounter(), "BeginEncounter", state);
                    ResolveCurrentWaveToBoundary(state);
                    break;

                case ReviewCaptureState.Wave2Active:
                    PrepareWave2Active(state);
                    break;

                case ReviewCaptureState.Interrupted:
                    PrepareWave2Active(state);
                    RequireAction(controller.InterruptReview(), "InterruptReview", state);
                    break;

                case ReviewCaptureState.Completed:
                    CompleteAllWaves(state);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }

            controller.RefreshCurrentView();
            ValidateExpectedRuntimeState(state);
        }

        private static void ValidateFreshSession()
        {
            ValidateControllerProfileAndArrays();
            ValidateProfileProjection();
            StageEncounterPlanReviewSession session = controller.Session;
            if (session.State != StageEncounterPlanReviewState.Ready
                || session.CurrentWaveIndex != -1
                || session.ClearedWaveCount != 0
                || session.CurrentRemainingCombatantCount != 0
                || session.AttemptGeneration != 1
                || session.CompletionCount != 0
                || session.InterruptionCount != 0)
            {
                throw new InvalidOperationException(
                    "CF-01 ReloadProfile did not create one clean deterministic session.");
            }

            EnsureNoActiveStageRun("fresh review session");
        }

        private static void PrepareWave2Active(ReviewCaptureState target)
        {
            RequireAction(controller.BeginEncounter(), "BeginEncounter", target);
            ResolveCurrentWaveToBoundary(target);
            if (controller.Session.State != StageEncounterPlanReviewState.WaveTransition
                || controller.Session.CurrentWaveIndex != 0)
            {
                throw new InvalidOperationException(
                    $"{target} preparation did not stop at the Wave 1 transition.");
            }

            RequireAction(controller.AdvanceWave(), "AdvanceWave", target);
        }

        private static void CompleteAllWaves(ReviewCaptureState target)
        {
            RequireAction(controller.BeginEncounter(), "BeginEncounter", target);
            int guard = 0;
            while (controller.Session.State != StageEncounterPlanReviewState.Completed)
            {
                if (++guard > 128)
                {
                    throw new InvalidOperationException(
                        "CF-01 completion preparation exceeded its deterministic guard.");
                }

                if (controller.Session.State == StageEncounterPlanReviewState.WaveActive)
                {
                    ResolveCurrentWaveToBoundary(target);
                    continue;
                }

                if (controller.Session.State == StageEncounterPlanReviewState.WaveTransition)
                {
                    RequireAction(controller.AdvanceWave(), "AdvanceWave", target);
                    continue;
                }

                throw new InvalidOperationException(
                    $"Unexpected CF-01 state {controller.Session.State} during completion.");
            }
        }

        private static void ResolveCurrentWaveToBoundary(ReviewCaptureState target)
        {
            int initialRemaining = controller.Session.CurrentRemainingCombatantCount;
            int guard = 0;
            while (controller.Session.State == StageEncounterPlanReviewState.WaveActive)
            {
                if (++guard > Mathf.Max(1, initialRemaining + 1))
                {
                    throw new InvalidOperationException(
                        $"CF-01 wave resolution exceeded its authored count for {target}.");
                }

                ResolveOneNextSpawn(target);
            }
        }

        private static void ResolveOneNextSpawn(ReviewCaptureState target)
        {
            StageEncounterPlanReviewSession session = controller.Session;
            if (!session.TryGetNextUnresolvedSpawn(
                    out StageEncounterPlanProfile.SpawnDefinition spawn,
                    out int remainingCount)
                || spawn == null
                || remainingCount <= 0)
            {
                throw new InvalidOperationException(
                    $"Session next-spawn API returned no deterministic spawn for {target}.");
            }

            int before = session.CurrentRemainingCombatantCount;
            RequireAction(controller.ResolveCurrentCombatant(), "ResolveCurrentCombatant", target);
            if (session.CurrentRemainingCombatantCount != before - 1)
            {
                throw new InvalidOperationException(
                    $"Resolving `{spawn.SpawnId}` did not decrement exactly one combatant.");
            }
        }

        private static void ValidateExpectedRuntimeState(ReviewCaptureState state)
        {
            ValidateControllerProfileAndArrays();
            ValidateProfileProjection();
            EnsureNoActiveStageRun(state.ToString());

            StageEncounterPlanReviewSession session = controller.Session;
            if (controller.CurrentState != session.State
                || session.WaveCount != ExpectedWaveCount
                || session.AttemptGeneration != 1)
            {
                throw new InvalidOperationException(
                    $"CF-01 controller/session identity drifted in {state}.");
            }

            int wave1Total = profile.GetWave(0).TotalCombatantCount;
            int wave2Total = profile.GetWave(1).TotalCombatantCount;
            ExpectedRuntimeState expected = state switch
            {
                ReviewCaptureState.Ready => new ExpectedRuntimeState(
                    StageEncounterPlanReviewState.Ready,
                    -1,
                    0,
                    0,
                    0,
                    0,
                    false,
                    StageEncounterWaveReviewStatus.Pending,
                    StageEncounterWaveReviewStatus.Pending,
                    StageEncounterWaveReviewStatus.Pending),
                ReviewCaptureState.Wave1Active => new ExpectedRuntimeState(
                    StageEncounterPlanReviewState.WaveActive,
                    0,
                    0,
                    wave1Total,
                    0,
                    0,
                    true,
                    StageEncounterWaveReviewStatus.Active,
                    StageEncounterWaveReviewStatus.Pending,
                    StageEncounterWaveReviewStatus.Pending),
                ReviewCaptureState.Wave1Partial => new ExpectedRuntimeState(
                    StageEncounterPlanReviewState.WaveActive,
                    0,
                    0,
                    wave1Total - 1,
                    0,
                    0,
                    true,
                    StageEncounterWaveReviewStatus.Active,
                    StageEncounterWaveReviewStatus.Pending,
                    StageEncounterWaveReviewStatus.Pending),
                ReviewCaptureState.Wave1Transition => new ExpectedRuntimeState(
                    StageEncounterPlanReviewState.WaveTransition,
                    0,
                    1,
                    0,
                    0,
                    0,
                    false,
                    StageEncounterWaveReviewStatus.Cleared,
                    StageEncounterWaveReviewStatus.Pending,
                    StageEncounterWaveReviewStatus.Pending),
                ReviewCaptureState.Wave2Active => new ExpectedRuntimeState(
                    StageEncounterPlanReviewState.WaveActive,
                    1,
                    1,
                    wave2Total,
                    0,
                    0,
                    true,
                    StageEncounterWaveReviewStatus.Cleared,
                    StageEncounterWaveReviewStatus.Active,
                    StageEncounterWaveReviewStatus.Pending),
                ReviewCaptureState.Interrupted => new ExpectedRuntimeState(
                    StageEncounterPlanReviewState.Interrupted,
                    1,
                    1,
                    wave2Total,
                    0,
                    1,
                    false,
                    StageEncounterWaveReviewStatus.Cleared,
                    StageEncounterWaveReviewStatus.Interrupted,
                    StageEncounterWaveReviewStatus.Pending),
                ReviewCaptureState.Completed => new ExpectedRuntimeState(
                    StageEncounterPlanReviewState.Completed,
                    2,
                    3,
                    0,
                    1,
                    0,
                    false,
                    StageEncounterWaveReviewStatus.Cleared,
                    StageEncounterWaveReviewStatus.Cleared,
                    StageEncounterWaveReviewStatus.Cleared),
                _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
            };

            if (session.State != expected.State
                || session.CurrentWaveIndex != expected.CurrentWaveIndex
                || session.ClearedWaveCount != expected.ClearedWaveCount
                || session.CurrentRemainingCombatantCount != expected.RemainingCount
                || session.CompletionCount != expected.CompletionCount
                || session.InterruptionCount != expected.InterruptionCount)
            {
                throw new InvalidOperationException(
                    $"CF-01 {state} counts mismatch. Actual state={session.State}, "
                    + $"wave={session.CurrentWaveIndex}, cleared={session.ClearedWaveCount}, "
                    + $"remaining={session.CurrentRemainingCombatantCount}, "
                    + $"completed={session.CompletionCount}, "
                    + $"interrupted={session.InterruptionCount}.");
            }

            StageEncounterWaveReviewStatus[] actualStatuses =
                session.CreateWaveStatusSnapshot();
            if (actualStatuses.Length != ExpectedWaveCount
                || actualStatuses[0] != expected.Wave1Status
                || actualStatuses[1] != expected.Wave2Status
                || actualStatuses[2] != expected.Wave3Status)
            {
                throw new InvalidOperationException(
                    $"CF-01 {state} wave status array is not exact: "
                    + string.Join("|", actualStatuses.Select(value => value.ToString())));
            }

            bool hasNext = session.TryGetNextUnresolvedSpawn(
                out StageEncounterPlanProfile.SpawnDefinition nextSpawn,
                out int nextRemaining);
            if (hasNext != expected.HasNextSpawn
                || (hasNext && (nextSpawn == null || nextRemaining <= 0)))
            {
                throw new InvalidOperationException(
                    $"CF-01 {state} next-spawn availability is not deterministic.");
            }
        }

        private static CaptureRecord CapturePlanFrame(CapturePlan plan)
        {
            UIResponsiveLayoutCatalog.BreakpointEntry responsiveLayout =
                ResolveAndValidateResponsiveLayout(plan);
            string path =
                $"{OutputDirectory}/{plan.Sequence:00}_{plan.State}_"
                + $"{plan.Width}x{plan.Height}.png";
            RenderTexture previousTarget = reviewCamera.targetTexture;
            RenderTexture previousActive = RenderTexture.active;
            RenderMode previousRenderMode = reviewCanvas.renderMode;
            Camera previousWorldCamera = reviewCanvas.worldCamera;
            float previousPlaneDistance = reviewCanvas.planeDistance;
            float previousCanvasScaleFactor = reviewCanvas.scaleFactor;
            float previousCameraAspect = reviewCamera.aspect;
            bool previousScalerEnabled = reviewCanvasScaler.enabled;
            Vector2 previousReferenceResolution =
                reviewCanvasScaler.referenceResolution;
            float previousMatchWidthOrHeight =
                reviewCanvasScaler.matchWidthOrHeight;
            bool previousResponsiveRootEnabled = responsiveRoot.enabled;
            bool previousSafeAreaEnabled = safeAreaRoot.enabled;
            RectTransform safeAreaRect = safeAreaRoot.transform as RectTransform
                ?? throw new InvalidOperationException(
                    "CF-01 safe-area RectTransform is unavailable.");
            Vector2 previousSafeAnchorMin = safeAreaRect.anchorMin;
            Vector2 previousSafeAnchorMax = safeAreaRect.anchorMax;
            Vector2 previousSafeOffsetMin = safeAreaRect.offsetMin;
            Vector2 previousSafeOffsetMax = safeAreaRect.offsetMax;

            var target = new RenderTexture(
                plan.Width,
                plan.Height,
                24,
                RenderTextureFormat.ARGB32,
                RenderTextureReadWrite.sRGB)
            {
                name = $"ContentFactoryEncounterPlanReviewQA_{plan.Width}x{plan.Height}",
                antiAliasing = 1,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            var image = new Texture2D(
                plan.Width,
                plan.Height,
                TextureFormat.RGBA32,
                mipChain: false,
                linear: false);

            try
            {
                target.Create();
                reviewCamera.targetTexture = target;
                reviewCamera.aspect = plan.Width / (float)plan.Height;
                reviewCanvas.renderMode = RenderMode.ScreenSpaceCamera;
                reviewCanvas.worldCamera = reviewCamera;
                reviewCanvas.planeDistance = Mathf.Max(
                    reviewCamera.nearClipPlane + 0.10f,
                    0.50f);
                // UIResponsiveRoot and UISafeAreaRoot normally write from Screen in
                // Update. Headless Screen dimensions need not match this RenderTexture,
                // so pause those late writers and apply the exact catalog selection for
                // the requested capture resolution ourselves.
                responsiveRoot.enabled = false;
                safeAreaRoot.enabled = false;
                reviewCanvasScaler.enabled = false;
                reviewCanvasScaler.referenceResolution =
                    responsiveLayout.ReferenceResolution;
                reviewCanvasScaler.matchWidthOrHeight =
                    responsiveLayout.MatchWidthOrHeight;
                reviewCanvas.scaleFactor = ResolveCanvasScaleFactor(
                    plan.Width,
                    plan.Height,
                    responsiveLayout.ReferenceResolution,
                    responsiveLayout.MatchWidthOrHeight);
                ApplyVirtualSafeArea(
                    safeAreaRect,
                    plan.Width,
                    plan.Height,
                    responsiveLayout.SafeAreaMode,
                    responsiveLayout.EdgeInset);

                Canvas.ForceUpdateCanvases();
                ValidateAppliedResponsiveLayout(
                    plan,
                    responsiveLayout,
                    safeAreaRect);
                ButtonAudit buttonAudit = AuditActionButtonsForCurrentResolution();
                reviewCamera.Render();
                RenderTexture.active = target;
                image.ReadPixels(
                    new Rect(0f, 0f, plan.Width, plan.Height),
                    0,
                    0,
                    recalculateMipMaps: false);
                image.Apply(updateMipmaps: false, makeNoLongerReadable: false);

                PixelAudit pixelAudit = AuditPixels(image);
                if (!pixelAudit.IsUsable)
                {
                    throw new InvalidOperationException(
                        $"Captured frame `{path}` is blank or has insufficient visual range "
                        + $"(mean={pixelAudit.MeanLuminance:0.0000}, "
                        + $"range={pixelAudit.LuminanceRange:0.0000}).");
                }

                byte[] png = image.EncodeToPNG();
                if (png == null || png.Length < 1024)
                {
                    throw new InvalidOperationException(
                        $"Captured frame `{path}` produced an invalid PNG payload.");
                }

                File.WriteAllBytes(path, png);
                StageEncounterPlanReviewSession session = controller.Session;
                bool hasNext = session.TryGetNextUnresolvedSpawn(
                    out StageEncounterPlanProfile.SpawnDefinition nextSpawn,
                    out int nextRemaining);
                return new CaptureRecord
                {
                    Sequence = plan.Sequence,
                    State = plan.State.ToString(),
                    Width = plan.Width,
                    Height = plan.Height,
                    Path = path,
                    FileBytes = png.LongLength,
                    MeanLuminance = pixelAudit.MeanLuminance,
                    LuminanceRange = pixelAudit.LuminanceRange,
                    ControllerState = controller.CurrentState.ToString(),
                    SessionState = session.State.ToString(),
                    CurrentWaveIndex = session.CurrentWaveIndex,
                    ClearedWaveCount = session.ClearedWaveCount,
                    RemainingCombatantCount = session.CurrentRemainingCombatantCount,
                    CompletionCount = session.CompletionCount,
                    InterruptionCount = session.InterruptionCount,
                    AttemptGeneration = session.AttemptGeneration,
                    WaveStatuses = string.Join(
                        "|",
                        session.CreateWaveStatusSnapshot().Select(value => value.ToString())),
                    NextSpawnId = hasNext && nextSpawn != null
                        ? nextSpawn.SpawnId
                        : string.Empty,
                    NextSpawnRemaining = hasNext ? nextRemaining : 0,
                    ProfilePath = ContentFactoryEncounterPlanReviewSetup.ProfilePath,
                    ProfileDigest = profile.CanonicalDigest,
                    ProtectedAssetDigest = SessionState.GetString(
                        ProtectedDigestBeforeKey,
                        string.Empty),
                    ExactProfileValidated = true,
                    ExactWaveArraysValidated = controller.HasExactWaveCardArrays,
                    ExactActionButtonMappingValidated = true,
                    ResponsiveCatalogPath =
                        ContentFactoryEncounterPlanReviewVisualQaCapture
                            .ResponsiveCatalogPath,
                    ResponsiveLayoutId = responsiveLayout.Id,
                    ResponsiveReferenceWidth =
                        responsiveLayout.ReferenceResolution.x,
                    ResponsiveReferenceHeight =
                        responsiveLayout.ReferenceResolution.y,
                    ResponsiveMatchWidthOrHeight =
                        responsiveLayout.MatchWidthOrHeight,
                    ResponsiveSafeAreaMode =
                        responsiveLayout.SafeAreaMode.ToString(),
                    ResponsiveEdgeInsetPixels = responsiveLayout.EdgeInset,
                    ResponsiveLayoutValidated = true,
                    ActionButtonCount = buttonAudit.ButtonCount,
                    MinimumButtonWidth = buttonAudit.MinimumWidth,
                    MinimumButtonHeight = buttonAudit.MinimumHeight,
                    MinimumTouchTargetValidated = buttonAudit.MinimumTargetValidated,
                    AllActionButtonsWithinCanvas = buttonAudit.AllWithinCanvas,
                    PersistentEventsAbsent = buttonAudit.PersistentEventsAbsent,
                    StageRunActive = StageRunRuntime.HasActiveContext,
                    RuntimeContractValidated = true
                };
            }
            finally
            {
                reviewCanvas.scaleFactor = previousCanvasScaleFactor;
                reviewCanvasScaler.referenceResolution = previousReferenceResolution;
                reviewCanvasScaler.matchWidthOrHeight = previousMatchWidthOrHeight;
                reviewCanvasScaler.enabled = previousScalerEnabled;
                safeAreaRect.anchorMin = previousSafeAnchorMin;
                safeAreaRect.anchorMax = previousSafeAnchorMax;
                safeAreaRect.offsetMin = previousSafeOffsetMin;
                safeAreaRect.offsetMax = previousSafeOffsetMax;
                safeAreaRoot.enabled = previousSafeAreaEnabled;
                responsiveRoot.enabled = previousResponsiveRootEnabled;
                reviewCanvas.renderMode = previousRenderMode;
                reviewCanvas.worldCamera = previousWorldCamera;
                reviewCanvas.planeDistance = previousPlaneDistance;
                reviewCamera.targetTexture = previousTarget;
                reviewCamera.aspect = previousCameraAspect;
                RenderTexture.active = previousActive;
                target.Release();
                UnityEngine.Object.DestroyImmediate(image);
                UnityEngine.Object.DestroyImmediate(target);
                Canvas.ForceUpdateCanvases();
            }
        }

        private static ButtonAudit AuditActionButtonsForCurrentResolution()
        {
            if (actionButtons.Length != ExpectedActionButtonCount)
            {
                throw new InvalidOperationException(
                    "CF-01 action-button count changed after runtime binding.");
            }

            float minimumWidth = float.PositiveInfinity;
            float minimumHeight = float.PositiveInfinity;
            bool allWithinCanvas = true;
            bool persistentEventsAbsent = true;
            Rect canvasRect = ((RectTransform)reviewCanvas.transform).rect;
            foreach (Button button in actionButtons)
            {
                if (button == null || button.transform is not RectTransform rectTransform)
                {
                    throw new InvalidOperationException(
                        "CF-01 action-button RectTransform became unavailable.");
                }

                Bounds bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(
                    reviewCanvas.transform,
                    rectTransform);
                float width = bounds.size.x * reviewCanvas.scaleFactor;
                float height = bounds.size.y * reviewCanvas.scaleFactor;
                minimumWidth = Mathf.Min(minimumWidth, width);
                minimumHeight = Mathf.Min(minimumHeight, height);
                allWithinCanvas &= bounds.min.x >= canvasRect.xMin - 0.5f
                    && bounds.max.x <= canvasRect.xMax + 0.5f
                    && bounds.min.y >= canvasRect.yMin - 0.5f
                    && bounds.max.y <= canvasRect.yMax + 0.5f;
                persistentEventsAbsent &= button.onClick.GetPersistentEventCount() == 0;
            }

            bool minimumTargetValidated = minimumWidth >= MinimumTouchTargetPixels
                && minimumHeight >= MinimumTouchTargetPixels;
            if (!minimumTargetValidated)
            {
                throw new InvalidOperationException(
                    $"CF-01 action targets must remain at least "
                    + $"{MinimumTouchTargetPixels:0}px; measured minimum "
                    + $"{minimumWidth:0.##}x{minimumHeight:0.##}px.");
            }

            if (!persistentEventsAbsent)
            {
                throw new InvalidOperationException(
                    "CF-01 action buttons gained persistent UnityEvent callbacks.");
            }

            if (!allWithinCanvas)
            {
                throw new InvalidOperationException(
                    "CF-01 action buttons must remain fully inside the rendered Canvas.");
            }

            return new ButtonAudit(
                actionButtons.Length,
                minimumWidth,
                minimumHeight,
                minimumTargetValidated,
                allWithinCanvas,
                persistentEventsAbsent);
        }

        private static UIResponsiveLayoutCatalog.BreakpointEntry
            ResolveAndValidateResponsiveLayout(CapturePlan plan)
        {
            if (responsiveCatalog == null
                || !responsiveCatalog.TryResolve(
                    new Vector2(plan.Width, plan.Height),
                    out UIResponsiveLayoutCatalog.BreakpointEntry entry))
            {
                throw new InvalidOperationException(
                    $"CF-01 responsive catalog did not resolve {plan.Width}x{plan.Height}.");
            }

            Vector2 reference = entry.ReferenceResolution;
            if (!string.Equals(
                    entry.Id,
                    ExpectedResponsiveLayoutId,
                    StringComparison.Ordinal)
                || !Approximately(reference.x, ExpectedReferenceWidth)
                || !Approximately(reference.y, ExpectedReferenceHeight)
                || !Approximately(
                    entry.MatchWidthOrHeight,
                    ExpectedMatchWidthOrHeight)
                || entry.SafeAreaMode != UISafeAreaMode.InsetsOnly
                || !Approximately(entry.EdgeInset, ExpectedEdgeInsetPixels))
            {
                throw new InvalidOperationException(
                    $"CF-01 {plan.Width}x{plan.Height} resolved `{entry.Id}` with "
                    + $"reference {reference.x:0.##}x{reference.y:0.##}, match "
                    + $"{entry.MatchWidthOrHeight:0.###}, safe-area "
                    + $"{entry.SafeAreaMode}, inset {entry.EdgeInset:0.##}; expected "
                    + $"`{ExpectedResponsiveLayoutId}`, {ExpectedReferenceWidth:0}x"
                    + $"{ExpectedReferenceHeight:0}, match "
                    + $"{ExpectedMatchWidthOrHeight:0.###}, InsetsOnly, "
                    + $"{ExpectedEdgeInsetPixels:0}px.");
            }

            return entry;
        }

        private static void ValidateAppliedResponsiveLayout(
            CapturePlan plan,
            UIResponsiveLayoutCatalog.BreakpointEntry entry,
            RectTransform safeAreaRect)
        {
            float expectedScale = ResolveCanvasScaleFactor(
                plan.Width,
                plan.Height,
                entry.ReferenceResolution,
                entry.MatchWidthOrHeight);
            float insetX = Mathf.Min(
                entry.EdgeInset,
                plan.Width * 0.45f) / Mathf.Max(1f, plan.Width);
            float insetY = Mathf.Min(
                entry.EdgeInset,
                plan.Height * 0.45f) / Mathf.Max(1f, plan.Height);
            Vector2 expectedAnchorMin = new Vector2(insetX, insetY);
            Vector2 expectedAnchorMax = new Vector2(1f - insetX, 1f - insetY);

            if (responsiveRoot.enabled
                || safeAreaRoot.enabled
                || reviewCanvasScaler.enabled
                || reviewCanvasScaler.referenceResolution
                    != entry.ReferenceResolution
                || !Approximately(
                    reviewCanvasScaler.matchWidthOrHeight,
                    entry.MatchWidthOrHeight)
                || !Approximately(reviewCanvas.scaleFactor, expectedScale)
                || !Approximately(safeAreaRect.anchorMin, expectedAnchorMin)
                || !Approximately(safeAreaRect.anchorMax, expectedAnchorMax)
                || safeAreaRect.offsetMin != Vector2.zero
                || safeAreaRect.offsetMax != Vector2.zero)
            {
                throw new InvalidOperationException(
                    $"CF-01 catalog layout `{entry.Id}` was not applied exactly for "
                    + $"{plan.Width}x{plan.Height}; a headless Screen writer may have "
                    + "overwritten the capture layout.");
            }
        }

        private static void ApplyVirtualSafeArea(
            RectTransform safeAreaRect,
            int width,
            int height,
            UISafeAreaMode mode,
            float insetPixels)
        {
            if (mode != UISafeAreaMode.InsetsOnly)
            {
                throw new InvalidOperationException(
                    $"CF-01 visual QA requires InsetsOnly; resolved {mode}.");
            }

            float horizontalInset = Mathf.Min(
                Mathf.Max(0f, insetPixels),
                Mathf.Max(1f, width) * 0.45f);
            float verticalInset = Mathf.Min(
                Mathf.Max(0f, insetPixels),
                Mathf.Max(1f, height) * 0.45f);
            float insetX = Mathf.Clamp01(horizontalInset / Mathf.Max(1f, width));
            float insetY = Mathf.Clamp01(verticalInset / Mathf.Max(1f, height));
            safeAreaRect.anchorMin = new Vector2(insetX, insetY);
            safeAreaRect.anchorMax = new Vector2(1f - insetX, 1f - insetY);
            safeAreaRect.offsetMin = Vector2.zero;
            safeAreaRect.offsetMax = Vector2.zero;
        }

        private static bool Approximately(float left, float right)
        {
            return Mathf.Abs(left - right) <= LayoutFloatTolerance;
        }

        private static bool Approximately(Vector2 left, Vector2 right)
        {
            return Approximately(left.x, right.x)
                && Approximately(left.y, right.y);
        }

        private static float ResolveCanvasScaleFactor(
            int width,
            int height,
            Vector2 referenceResolution,
            float matchWidthOrHeight)
        {
            float referenceWidth = Mathf.Max(1f, referenceResolution.x);
            float referenceHeight = Mathf.Max(1f, referenceResolution.y);
            float logWidth = Mathf.Log(width / referenceWidth, 2f);
            float logHeight = Mathf.Log(height / referenceHeight, 2f);
            float weighted = Mathf.Lerp(
                logWidth,
                logHeight,
                Mathf.Clamp01(matchWidthOrHeight));
            return Mathf.Pow(2f, weighted);
        }

        private static PixelAudit AuditPixels(Texture2D image)
        {
            const int HorizontalSamples = 32;
            const int VerticalSamples = 18;
            float minimum = 1f;
            float maximum = 0f;
            double total = 0d;
            int count = 0;
            for (int sampleY = 0; sampleY < VerticalSamples; sampleY++)
            {
                int y = Mathf.RoundToInt(
                    (image.height - 1) * (sampleY / (float)(VerticalSamples - 1)));
                for (int sampleX = 0; sampleX < HorizontalSamples; sampleX++)
                {
                    int x = Mathf.RoundToInt(
                        (image.width - 1) * (sampleX / (float)(HorizontalSamples - 1)));
                    Color pixel = image.GetPixel(x, y);
                    float luminance = (pixel.r * 0.2126f)
                        + (pixel.g * 0.7152f)
                        + (pixel.b * 0.0722f);
                    minimum = Mathf.Min(minimum, luminance);
                    maximum = Mathf.Max(maximum, luminance);
                    total += luminance;
                    count++;
                }
            }

            float mean = count > 0 ? (float)(total / count) : 0f;
            float range = Mathf.Max(0f, maximum - minimum);
            return new PixelAudit(
                mean,
                range,
                maximum > 0.045f && range > 0.018f && mean > 0.006f);
        }

        private static void CompleteCaptureSet()
        {
            List<string> issues = ValidateOutputSet();
            string digestBefore = SessionState.GetString(
                ProtectedDigestBeforeKey,
                string.Empty);
            string digestAfterCapture = TryComputeProtectedAssetDigest();
            if (string.IsNullOrWhiteSpace(digestBefore)
                || !string.Equals(
                    digestBefore,
                    digestAfterCapture,
                    StringComparison.Ordinal))
            {
                issues.Add(
                    "CF-01 protected-asset digest changed during Play-mode capture.");
            }

            bool capturePassed = issues.Count == 0;
            string failure = capturePassed ? string.Empty : string.Join("\n", issues);
            SessionState.SetString(FailureKey, failure);
            if (!TryWriteReports(
                automatedPassed: capturePassed,
                failure: capturePassed ? "EDIT-MODE POSTFLIGHT PENDING" : failure,
                setupBefore: SessionState.GetBool(SetupBeforeKey, false),
                setupAfter: false,
                digestBefore: digestBefore,
                digestAfter: digestAfterCapture,
                postflightPending: true,
                out string reportFailure))
            {
                issues.Add(reportFailure);
                capturePassed = false;
                failure = string.Join("\n", issues.Distinct());
                SessionState.SetString(FailureKey, failure);
            }

            SessionState.SetInt(
                PhaseKey,
                (int)(capturePassed
                    ? RunnerPhase.SuccessAwaitingEditMode
                    : RunnerPhase.FailureAwaitingEditMode));
            EditorApplication.ExitPlaymode();
        }

        private static List<string> ValidateOutputSet()
        {
            var issues = new List<string>();
            responsiveCatalog ??=
                AssetDatabase.LoadAssetAtPath<UIResponsiveLayoutCatalog>(
                    ResponsiveCatalogPath);
            if (Records.Count != ExpectedCaptureCount)
            {
                issues.Add(
                    $"Expected {ExpectedCaptureCount} records; found {Records.Count}.");
            }

            string[] pngFiles = Directory.Exists(OutputDirectory)
                ? Directory.GetFiles(OutputDirectory, "*.png", SearchOption.TopDirectoryOnly)
                : Array.Empty<string>();
            if (pngFiles.Length != ExpectedCaptureCount)
            {
                issues.Add(
                    $"Expected exactly {ExpectedCaptureCount} PNGs; found {pngFiles.Length}.");
            }

            string expectedDigest = SessionState.GetString(
                ProtectedDigestBeforeKey,
                string.Empty);
            foreach (CapturePlan plan in Plans)
            {
                UIResponsiveLayoutCatalog.BreakpointEntry expectedLayout;
                try
                {
                    expectedLayout = ResolveAndValidateResponsiveLayout(plan);
                }
                catch (Exception exception)
                {
                    issues.Add(exception.Message);
                    continue;
                }

                CaptureRecord record = Records.FirstOrDefault(
                    candidate => candidate.Sequence == plan.Sequence);
                if (record == null)
                {
                    issues.Add($"Missing record {plan.Sequence:00} / {plan.State}.");
                    continue;
                }

                string expectedPath =
                    $"{OutputDirectory}/{plan.Sequence:00}_{plan.State}_"
                    + $"{plan.Width}x{plan.Height}.png";
                if (!string.Equals(
                        Path.GetFullPath(record.Path ?? string.Empty),
                        Path.GetFullPath(expectedPath),
                        StringComparison.OrdinalIgnoreCase))
                {
                    issues.Add(
                        $"Capture record {plan.Sequence:00} path is not the exact plan path.");
                }

                if (!File.Exists(expectedPath))
                {
                    issues.Add($"Capture file missing: `{expectedPath}`.");
                    continue;
                }

                byte[] png = File.ReadAllBytes(expectedPath);
                if (record.FileBytes != png.LongLength || png.LongLength < 1024)
                {
                    issues.Add(
                        $"Capture {plan.Sequence:00} file length does not match its record.");
                }
                var decoded = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                try
                {
                    if (!decoded.LoadImage(png, markNonReadable: false))
                    {
                        issues.Add($"Could not decode `{expectedPath}`.");
                    }
                    else if (decoded.width != plan.Width || decoded.height != plan.Height)
                    {
                        issues.Add(
                            $"Capture `{expectedPath}` is {decoded.width}x{decoded.height}; "
                            + $"expected {plan.Width}x{plan.Height}.");
                    }
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(decoded);
                }

                if (!string.Equals(
                        record.State,
                        plan.State.ToString(),
                        StringComparison.Ordinal)
                    || record.Width != plan.Width
                    || record.Height != plan.Height)
                {
                    issues.Add($"Capture record {plan.Sequence:00} plan identity drifted.");
                }

                if (!record.RuntimeContractValidated
                    || !record.ExactProfileValidated
                    || !record.ExactWaveArraysValidated
                    || !record.ExactActionButtonMappingValidated
                    || !record.ResponsiveLayoutValidated
                    || !record.MinimumTouchTargetValidated
                    || !record.AllActionButtonsWithinCanvas
                    || !record.PersistentEventsAbsent
                    || record.ActionButtonCount != ExpectedActionButtonCount
                    || record.StageRunActive)
                {
                    issues.Add(
                        $"Capture {plan.Sequence:00} did not retain the CF-01 runtime boundary.");
                }

                if (!string.Equals(
                        record.ResponsiveCatalogPath,
                        ResponsiveCatalogPath,
                        StringComparison.Ordinal)
                    || !string.Equals(
                        record.ResponsiveLayoutId,
                        expectedLayout.Id,
                        StringComparison.Ordinal)
                    || !Approximately(
                        record.ResponsiveReferenceWidth,
                        expectedLayout.ReferenceResolution.x)
                    || !Approximately(
                        record.ResponsiveReferenceHeight,
                        expectedLayout.ReferenceResolution.y)
                    || !Approximately(
                        record.ResponsiveMatchWidthOrHeight,
                        expectedLayout.MatchWidthOrHeight)
                    || !string.Equals(
                        record.ResponsiveSafeAreaMode,
                        expectedLayout.SafeAreaMode.ToString(),
                        StringComparison.Ordinal)
                    || !Approximately(
                        record.ResponsiveEdgeInsetPixels,
                        expectedLayout.EdgeInset))
                {
                    issues.Add(
                        $"Capture {plan.Sequence:00} responsive layout evidence is stale.");
                }

                if (string.IsNullOrWhiteSpace(expectedDigest)
                    || !string.Equals(
                        record.ProtectedAssetDigest,
                        expectedDigest,
                        StringComparison.Ordinal))
                {
                    issues.Add(
                        $"Capture {plan.Sequence:00} protected digest is missing or stale.");
                }
            }

            foreach (ReviewCaptureState state in Enum.GetValues(typeof(ReviewCaptureState)))
            {
                int count = Records.Count(record => string.Equals(
                    record.State,
                    state.ToString(),
                    StringComparison.Ordinal));
                if (count != 3)
                {
                    issues.Add($"State {state} has {count} captures; expected 3.");
                }
            }

            foreach (string resolution in new[]
                     {
                         "1920x1080",
                         "2400x1080",
                         "2520x1080"
                     })
            {
                int count = Records.Count(
                    record => $"{record.Width}x{record.Height}" == resolution);
                if (count != 7)
                {
                    issues.Add($"Resolution {resolution} has {count} captures; expected 7.");
                }
            }

            return issues;
        }

        private static void ResetOutputArtifacts()
        {
            Directory.CreateDirectory(OutputDirectory);
            foreach (string path in Directory.GetFiles(
                         OutputDirectory,
                         "*.png",
                         SearchOption.TopDirectoryOnly))
            {
                File.Delete(path);
            }

            if (File.Exists(ManifestPath))
            {
                File.Delete(ManifestPath);
            }

            if (File.Exists(ReportPath))
            {
                File.Delete(ReportPath);
            }
        }

        private static void FinishWithFailure(string failure)
        {
            string resolved = string.IsNullOrWhiteSpace(failure)
                ? "Unknown CF-01 visual QA failure."
                : failure;
            SessionState.SetString(FailureKey, resolved);
            if (!TryWriteReports(
                automatedPassed: false,
                failure: resolved,
                setupBefore: SessionState.GetBool(SetupBeforeKey, false),
                setupAfter: false,
                digestBefore: SessionState.GetString(
                    ProtectedDigestBeforeKey,
                    string.Empty),
                digestAfter: TryComputeProtectedAssetDigest(),
                postflightPending: true,
                out string reportFailure))
            {
                resolved += "\n" + reportFailure;
                SessionState.SetString(FailureKey, resolved);
            }
            Debug.LogError(
                "[ContentFactoryEncounterPlanReviewVisualQA] BATCH_CAPTURE_CHECK_FAIL\n"
                + resolved);
            SessionState.SetInt(PhaseKey, (int)RunnerPhase.FailureAwaitingEditMode);
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorApplication.ExitPlaymode();
            }
        }

        private static void WriteReports(
            bool automatedPassed,
            string failure,
            bool setupBefore,
            bool setupAfter,
            string digestBefore,
            string digestAfter,
            bool postflightPending)
        {
            Directory.CreateDirectory(OutputDirectory);
            bool digestStable = !string.IsNullOrWhiteSpace(digestBefore)
                && string.Equals(digestBefore, digestAfter, StringComparison.Ordinal);
            var manifest = new CaptureManifest
            {
                Scene = ScenePath,
                Profile = ContentFactoryEncounterPlanReviewSetup.ProfilePath,
                GeneratedUtc = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture),
                AutomatedPassed = automatedPassed,
                HumanReviewRequired = true,
                HumanReviewed = false,
                PostflightPending = postflightPending,
                Failure = failure ?? string.Empty,
                ExpectedCaptureCount = ExpectedCaptureCount,
                SetupVerificationBefore = setupBefore,
                SetupVerificationAfter = setupAfter,
                ProtectedDigestBefore = digestBefore ?? string.Empty,
                ProtectedDigestAfter = digestAfter ?? string.Empty,
                ProtectedDigestStable = digestStable,
                StatePreparationBoundary =
                    "ResetReview/ReloadProfile/BeginEncounter/ResolveCurrentCombatant/"
                    + "AdvanceWave/InterruptReview plus read-only Session next-spawn query",
                RuntimeBoundary =
                    "Review-only session; no StageRun admission, runtime spawn, combat outcome, "
                    + "reward, route, persistence, or server dispatch",
                Captures = Records.OrderBy(record => record.Sequence).ToArray()
            };
            File.WriteAllText(
                ManifestPath,
                JsonUtility.ToJson(manifest, prettyPrint: true),
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

            var report = new StringBuilder();
            report.AppendLine("# CF-01 Content Factory Encounter Plan Visual QA");
            report.AppendLine();
            report.AppendLine(
                postflightPending
                    ? "Automated capture check: POSTFLIGHT PENDING"
                    : automatedPassed
                        ? "Automated capture check: PASS"
                        : "Automated capture check: FAIL");
            report.AppendLine("Human visual review: PENDING (must be recorded separately)");
            report.AppendLine();
            report.AppendLine($"- Scene: `{ScenePath}`");
            report.AppendLine(
                $"- Profile: `{ContentFactoryEncounterPlanReviewSetup.ProfilePath}`");
            report.AppendLine($"- Output: `{OutputDirectory}`");
            report.AppendLine($"- Captures: `{Records.Count}` / `{ExpectedCaptureCount}`");
            report.AppendLine("- Resolutions: `1920x1080`, `2400x1080`, `2520x1080`");
            report.AppendLine(
                "- States: Ready, Wave1Active, Wave1Partial, Wave1Transition, "
                + "Wave2Active, Interrupted, Completed");
            report.AppendLine(
                "- State preparation: ResetReview + fresh exact-profile session + public "
                + "controller actions and read-only next-spawn query");
            report.AppendLine(
                "- Runtime checks: exact profile, exact three-wave arrays, deterministic "
                + "session counts, 48px targets, no persistent button events, no StageRun");
            report.AppendLine(
                "- Layout checks: exact DB_UIResponsiveLayouts selection manually applied "
                + "while late headless writers are paused; all five action buttons fully "
                + "inside the rendered Canvas");
            report.AppendLine("- Rendering: Camera RenderTexture + ScreenSpaceCamera Canvas");
            report.AppendLine(
                $"- Setup verification: before=`{setupBefore}`; after=`{setupAfter}`");
            report.AppendLine($"- Protected digest before: `{digestBefore}`");
            report.AppendLine($"- Protected digest after: `{digestAfter}`");
            report.AppendLine($"- Protected digest stable: `{digestStable}`");
            report.AppendLine("- HumanReviewRequired: `true`; HumanReviewed: `false`");
            report.AppendLine();
            if (!string.IsNullOrWhiteSpace(failure))
            {
                report.AppendLine("## Failure / pending state");
                report.AppendLine();
                report.AppendLine("```text");
                report.AppendLine(failure.Trim());
                report.AppendLine("```");
                report.AppendLine();
            }

            report.AppendLine("## Captures");
            report.AppendLine();
            report.AppendLine(
                "| # | State | Resolution | Session | Wave | Cleared / Remaining | "
                + "Complete / Interrupt | Min target | Luma mean/range | Path |");
            report.AppendLine(
                "|---:|---|---:|---|---:|---:|---:|---:|---:|---|");
            foreach (CaptureRecord record in Records.OrderBy(item => item.Sequence))
            {
                report.AppendLine(
                    $"| {record.Sequence:00} | {record.State} | "
                    + $"{record.Width}x{record.Height} | {record.SessionState} | "
                    + $"{record.CurrentWaveIndex} | {record.ClearedWaveCount} / "
                    + $"{record.RemainingCombatantCount} | {record.CompletionCount} / "
                    + $"{record.InterruptionCount} | {record.MinimumButtonWidth:0.#}x"
                    + $"{record.MinimumButtonHeight:0.#} | "
                    + $"{record.MeanLuminance:0.0000}/{record.LuminanceRange:0.0000} | "
                    + $"`{record.Path}` |");
            }

            report.AppendLine();
            report.AppendLine(
                "Automated success does not attest composition, contrast, hierarchy, or "
                + "visual polish. Inspect all 21 PNGs before recording human review.");
            File.WriteAllText(
                ReportPath,
                report.ToString(),
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        }

        private static bool TryWriteReports(
            bool automatedPassed,
            string failure,
            bool setupBefore,
            bool setupAfter,
            string digestBefore,
            string digestAfter,
            bool postflightPending,
            out string writeFailure)
        {
            try
            {
                WriteReports(
                    automatedPassed,
                    failure,
                    setupBefore,
                    setupAfter,
                    digestBefore,
                    digestAfter,
                    postflightPending);
                writeFailure = string.Empty;
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                writeFailure = "CF-01 report I/O failed: " + exception;
                return false;
            }
        }

        private static void FinalizeEditorSession(bool capturePhasePassed)
        {
            bool exitEditor = SessionState.GetBool(BatchExitKey, false);
            bool setupBefore = SessionState.GetBool(SetupBeforeKey, false);
            string digestBefore = SessionState.GetString(
                ProtectedDigestBeforeKey,
                string.Empty);
            string failure = SessionState.GetString(FailureKey, string.Empty);
            bool setupAfter = false;
            string digestAfter = string.Empty;
            bool success = false;
            string finalFailure = string.Empty;
            var issues = new List<string>();
            try
            {
                try
                {
                    CaptureManifest pendingManifest = ReadManifestStrict();
                    RestoreRecordsFromManifest(pendingManifest);
                    issues.AddRange(ValidateManifestEnvelope(
                        pendingManifest,
                        expectedAutomatedPassed: capturePhasePassed,
                        expectedPostflightPending: true));
                }
                catch (Exception exception)
                {
                    Records.Clear();
                    issues.Add(
                        "Edit-mode manifest recovery failed closed: " + exception.Message);
                    Debug.LogException(exception);
                }

                try
                {
                    ContentFactoryEncounterPlanReviewSetup.RunBatchVerification();
                    setupAfter = true;
                    digestAfter =
                        ContentFactoryEncounterPlanReviewSetup.ComputeProtectedAssetDigest();
                }
                catch (Exception exception)
                {
                    issues.Add("Edit-mode setup postflight failed: " + exception.Message);
                    Debug.LogException(exception);
                }

                if (!setupBefore)
                {
                    issues.Add("Setup verification did not pass before capture.");
                }

                if (!setupAfter)
                {
                    issues.Add("Setup verification did not pass after capture.");
                }

                if (string.IsNullOrWhiteSpace(digestBefore)
                    || !string.Equals(
                        digestBefore,
                        digestAfter,
                        StringComparison.Ordinal))
                {
                    issues.Add(
                        "Protected-asset digest differs before/after CF-01 visual QA.");
                }

                if (!string.IsNullOrWhiteSpace(failure))
                {
                    issues.Insert(0, failure);
                }

                try
                {
                    issues.AddRange(ValidateOutputSet());
                }
                catch (Exception exception)
                {
                    issues.Add(
                        "Edit-mode output-set revalidation failed: " + exception.Message);
                    Debug.LogException(exception);
                }

                success = capturePhasePassed && issues.Count == 0;
                finalFailure = success
                    ? string.Empty
                    : string.Join("\n", issues.Distinct());
                if (!TryWriteReports(
                    success,
                    finalFailure,
                    setupBefore,
                    setupAfter,
                    digestBefore,
                    digestAfter,
                    postflightPending: false,
                    out string reportFailure))
                {
                    issues.Add(reportFailure);
                    success = false;
                    finalFailure = string.Join("\n", issues.Distinct());
                }

                // A PASS is emitted only after the final manifest itself is readable,
                // records automated success, contains exactly 21 non-null records, and
                // those records/PNGs survive the same output-set checks a second time.
                if (success)
                {
                    var persistedIssues = new List<string>();
                    try
                    {
                        CaptureManifest finalManifest = ReadManifestStrict();
                        RestoreRecordsFromManifest(finalManifest);
                        persistedIssues.AddRange(ValidateManifestEnvelope(
                            finalManifest,
                            expectedAutomatedPassed: true,
                            expectedPostflightPending: false));
                        persistedIssues.AddRange(ValidateOutputSet());
                    }
                    catch (Exception exception)
                    {
                        persistedIssues.Add(
                            "Final manifest/output readback failed: " + exception.Message);
                        Debug.LogException(exception);
                    }

                    if (persistedIssues.Count > 0)
                    {
                        issues.AddRange(persistedIssues);
                        success = false;
                        finalFailure = string.Join("\n", issues.Distinct());
                        TryWriteReports(
                            automatedPassed: false,
                            failure: finalFailure,
                            setupBefore: setupBefore,
                            setupAfter: setupAfter,
                            digestBefore: digestBefore,
                            digestAfter: digestAfter,
                            postflightPending: false,
                            out _);
                    }
                }

                if (success)
                {
                    Debug.Log(
                        "[ContentFactoryEncounterPlanReviewVisualQA] "
                        + "BATCH_CAPTURE_CHECK_PASS "
                        + $"captures={Records.Count} humanReview=pending "
                        + $"output=`{OutputDirectory}`");
                }
                else
                {
                    Debug.LogError(
                        "[ContentFactoryEncounterPlanReviewVisualQA] "
                        + "BATCH_CAPTURE_CHECK_FAIL\n" + finalFailure);
                }
            }
            catch (Exception exception)
            {
                success = false;
                issues.Add("Unhandled CF-01 finalization failure: " + exception);
                finalFailure = string.Join("\n", issues.Distinct());
                Debug.LogException(exception);
                TryWriteReports(
                    automatedPassed: false,
                    failure: finalFailure,
                    setupBefore: setupBefore,
                    setupAfter: setupAfter,
                    digestBefore: digestBefore,
                    digestAfter: digestAfter,
                    postflightPending: false,
                    out _);
            }
            finally
            {
                ClearSessionState();
                ResetRuntimeFields();
                if (exitEditor)
                {
                    EditorApplication.Exit(success ? 0 : 1);
                }
            }
        }

        private static CaptureManifest ReadManifestStrict()
        {
            if (!File.Exists(ManifestPath))
            {
                throw new FileNotFoundException(
                    "CF-01 capture manifest is missing.",
                    ManifestPath);
            }

            CaptureManifest manifest = JsonUtility.FromJson<CaptureManifest>(
                File.ReadAllText(ManifestPath));
            return manifest
                ?? throw new InvalidDataException(
                    "CF-01 capture manifest deserialized to null.");
        }

        private static void RestoreRecordsFromManifest(CaptureManifest manifest)
        {
            CaptureRecord[] recovered = manifest.Captures
                ?? throw new InvalidDataException(
                    "CF-01 capture manifest has no capture array.");
            if (recovered.Any(record => record == null))
            {
                throw new InvalidDataException(
                    "CF-01 capture manifest contains a null record.");
            }

            Records.Clear();
            Records.AddRange(recovered);
        }

        private static List<string> ValidateManifestEnvelope(
            CaptureManifest manifest,
            bool expectedAutomatedPassed,
            bool expectedPostflightPending)
        {
            var issues = new List<string>();
            if (!string.Equals(manifest.Scene, ScenePath, StringComparison.Ordinal)
                || !string.Equals(
                    manifest.Profile,
                    ContentFactoryEncounterPlanReviewSetup.ProfilePath,
                    StringComparison.Ordinal))
            {
                issues.Add("CF-01 manifest scene/profile identity drifted.");
            }

            if (manifest.ExpectedCaptureCount != ExpectedCaptureCount
                || manifest.Captures == null
                || manifest.Captures.Length != ExpectedCaptureCount)
            {
                issues.Add(
                    "CF-01 manifest must contain exactly "
                    + $"{ExpectedCaptureCount} capture records.");
            }

            if (manifest.AutomatedPassed != expectedAutomatedPassed)
            {
                issues.Add(
                    "CF-01 manifest automated-pass state does not match finalization.");
            }

            if (manifest.PostflightPending != expectedPostflightPending)
            {
                issues.Add("CF-01 manifest postflight state does not match finalization.");
            }

            if (!manifest.HumanReviewRequired || manifest.HumanReviewed)
            {
                issues.Add("CF-01 manifest human-review boundary drifted.");
            }

            return issues;
        }

        private static void HandleLaunchFailure(
            Exception exception,
            bool exitEditorWhenFinished)
        {
            Debug.LogException(exception);
            string digestBefore = SessionState.GetString(
                ProtectedDigestBeforeKey,
                string.Empty);
            try
            {
                TryWriteReports(
                    automatedPassed: false,
                    failure: exception.ToString(),
                    setupBefore: SessionState.GetBool(SetupBeforeKey, false),
                    setupAfter: false,
                    digestBefore: digestBefore,
                    digestAfter: TryComputeProtectedAssetDigest(),
                    postflightPending: false,
                    out _);
            }
            finally
            {
                ClearSessionState();
                ResetRuntimeFields();
                if (exitEditorWhenFinished)
                {
                    EditorApplication.Exit(1);
                }
            }
        }

        private static bool HasTimedOut()
        {
            string raw = SessionState.GetString(StartedUtcTicksKey, string.Empty);
            return long.TryParse(
                    raw,
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out long ticks)
                && (DateTime.UtcNow - new DateTime(ticks, DateTimeKind.Utc)).TotalSeconds
                    > LaunchTimeoutSeconds;
        }

        private static void ResetRuntimeFields()
        {
            controller = null;
            profile = null;
            reviewCamera = null;
            reviewCanvas = null;
            reviewCanvasScaler = null;
            responsiveRoot = null;
            safeAreaRoot = null;
            responsiveCatalog = null;
            actionButtons = Array.Empty<Button>();
            planIndex = 0;
            readyAtFrame = 0;
            statePrepared = false;
            runtimeInitialized = false;
            Records.Clear();
        }

        private static void ClearSessionState()
        {
            SessionState.EraseBool(ActiveKey);
            SessionState.EraseBool(BatchExitKey);
            SessionState.EraseInt(PhaseKey);
            SessionState.EraseString(FailureKey);
            SessionState.EraseString(StartedUtcTicksKey);
            SessionState.EraseBool(SetupBeforeKey);
            SessionState.EraseString(ProtectedDigestBeforeKey);
        }

        private static CapturePlan[] BuildCapturePlans()
        {
            var plans = new List<CapturePlan>(ExpectedCaptureCount);
            int sequence = 1;
            AddResolution(plans, ref sequence, 1920, 1080);
            AddResolution(plans, ref sequence, 2400, 1080);
            AddResolution(plans, ref sequence, 2520, 1080);
            return plans.ToArray();
        }

        private static void AddResolution(
            List<CapturePlan> plans,
            ref int sequence,
            int width,
            int height)
        {
            foreach (ReviewCaptureState state in Enum.GetValues(
                         typeof(ReviewCaptureState)))
            {
                plans.Add(new CapturePlan(sequence++, width, height, state));
            }
        }

        private static void RequireAction(
            bool accepted,
            string action,
            ReviewCaptureState target)
        {
            if (!accepted)
            {
                throw new InvalidOperationException(
                    $"Public controller action `{action}` was rejected while preparing "
                    + $"{target}.");
            }
        }

        private static void EnsureNoActiveStageRun(string context)
        {
            if (StageRunRuntime.HasActiveContext)
            {
                throw new InvalidOperationException(
                    $"CF-01 {context} unexpectedly created an active StageRun context.");
            }
        }

        private static void RequireGeneratedInput(string assetPath, string label)
        {
            string absolutePath = AssetPathToAbsolutePath(assetPath);
            if (!File.Exists(absolutePath))
            {
                throw new FileNotFoundException(
                    $"Generate the CF-01 {label} before visual QA.",
                    absolutePath);
            }
        }

        private static string TryComputeProtectedAssetDigest()
        {
            try
            {
                return ContentFactoryEncounterPlanReviewSetup.ComputeProtectedAssetDigest()
                    ?? string.Empty;
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                return string.Empty;
            }
        }

        private static T FindSingleInScene<T>(Scene scene) where T : Component
        {
            T[] components = FindAllInScene<T>(scene);
            if (components.Length != 1)
            {
                throw new InvalidOperationException(
                    $"Scene `{scene.path}` must contain exactly one {typeof(T).Name}; "
                    + $"found {components.Length}.");
            }

            return components[0];
        }

        private static T[] FindAllInScene<T>(Scene scene) where T : Component
        {
            var components = new List<T>();
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                components.AddRange(
                    root.GetComponentsInChildren<T>(includeInactive: true));
            }

            return components.ToArray();
        }

        private static string AssetPathToAbsolutePath(string assetPath)
        {
            string projectRoot = Path.GetDirectoryName(Application.dataPath)
                ?? throw new InvalidOperationException("Unity project root is unavailable.");
            return Path.GetFullPath(
                Path.Combine(
                    projectRoot,
                    assetPath.Replace('/', Path.DirectorySeparatorChar)));
        }

        private readonly struct CapturePlan
        {
            public CapturePlan(
                int sequence,
                int width,
                int height,
                ReviewCaptureState state)
            {
                Sequence = sequence;
                Width = width;
                Height = height;
                State = state;
            }

            public int Sequence { get; }
            public int Width { get; }
            public int Height { get; }
            public ReviewCaptureState State { get; }
        }

        private readonly struct ExpectedRuntimeState
        {
            public ExpectedRuntimeState(
                StageEncounterPlanReviewState state,
                int currentWaveIndex,
                int clearedWaveCount,
                int remainingCount,
                int completionCount,
                int interruptionCount,
                bool hasNextSpawn,
                StageEncounterWaveReviewStatus wave1Status,
                StageEncounterWaveReviewStatus wave2Status,
                StageEncounterWaveReviewStatus wave3Status)
            {
                State = state;
                CurrentWaveIndex = currentWaveIndex;
                ClearedWaveCount = clearedWaveCount;
                RemainingCount = remainingCount;
                CompletionCount = completionCount;
                InterruptionCount = interruptionCount;
                HasNextSpawn = hasNextSpawn;
                Wave1Status = wave1Status;
                Wave2Status = wave2Status;
                Wave3Status = wave3Status;
            }

            public StageEncounterPlanReviewState State { get; }
            public int CurrentWaveIndex { get; }
            public int ClearedWaveCount { get; }
            public int RemainingCount { get; }
            public int CompletionCount { get; }
            public int InterruptionCount { get; }
            public bool HasNextSpawn { get; }
            public StageEncounterWaveReviewStatus Wave1Status { get; }
            public StageEncounterWaveReviewStatus Wave2Status { get; }
            public StageEncounterWaveReviewStatus Wave3Status { get; }
        }

        private readonly struct PixelAudit
        {
            public PixelAudit(
                float meanLuminance,
                float luminanceRange,
                bool isUsable)
            {
                MeanLuminance = meanLuminance;
                LuminanceRange = luminanceRange;
                IsUsable = isUsable;
            }

            public float MeanLuminance { get; }
            public float LuminanceRange { get; }
            public bool IsUsable { get; }
        }

        private readonly struct ButtonAudit
        {
            public ButtonAudit(
                int buttonCount,
                float minimumWidth,
                float minimumHeight,
                bool minimumTargetValidated,
                bool allWithinCanvas,
                bool persistentEventsAbsent)
            {
                ButtonCount = buttonCount;
                MinimumWidth = minimumWidth;
                MinimumHeight = minimumHeight;
                MinimumTargetValidated = minimumTargetValidated;
                AllWithinCanvas = allWithinCanvas;
                PersistentEventsAbsent = persistentEventsAbsent;
            }

            public int ButtonCount { get; }
            public float MinimumWidth { get; }
            public float MinimumHeight { get; }
            public bool MinimumTargetValidated { get; }
            public bool AllWithinCanvas { get; }
            public bool PersistentEventsAbsent { get; }
        }

        [Serializable]
        private sealed class CaptureRecord
        {
            public int Sequence;
            public string State;
            public int Width;
            public int Height;
            public string Path;
            public long FileBytes;
            public float MeanLuminance;
            public float LuminanceRange;
            public string ControllerState;
            public string SessionState;
            public int CurrentWaveIndex;
            public int ClearedWaveCount;
            public int RemainingCombatantCount;
            public int CompletionCount;
            public int InterruptionCount;
            public int AttemptGeneration;
            public string WaveStatuses;
            public string NextSpawnId;
            public int NextSpawnRemaining;
            public string ProfilePath;
            public string ProfileDigest;
            public string ProtectedAssetDigest;
            public bool ExactProfileValidated;
            public bool ExactWaveArraysValidated;
            public bool ExactActionButtonMappingValidated;
            public string ResponsiveCatalogPath;
            public string ResponsiveLayoutId;
            public float ResponsiveReferenceWidth;
            public float ResponsiveReferenceHeight;
            public float ResponsiveMatchWidthOrHeight;
            public string ResponsiveSafeAreaMode;
            public float ResponsiveEdgeInsetPixels;
            public bool ResponsiveLayoutValidated;
            public int ActionButtonCount;
            public float MinimumButtonWidth;
            public float MinimumButtonHeight;
            public bool MinimumTouchTargetValidated;
            public bool AllActionButtonsWithinCanvas;
            public bool PersistentEventsAbsent;
            public bool StageRunActive;
            public bool RuntimeContractValidated;
        }

        [Serializable]
        private sealed class CaptureManifest
        {
            public string Scene;
            public string Profile;
            public string GeneratedUtc;
            public bool AutomatedPassed;
            public bool HumanReviewRequired;
            public bool HumanReviewed;
            public bool PostflightPending;
            public string Failure;
            public int ExpectedCaptureCount;
            public bool SetupVerificationBefore;
            public bool SetupVerificationAfter;
            public string ProtectedDigestBefore;
            public string ProtectedDigestAfter;
            public bool ProtectedDigestStable;
            public string StatePreparationBoundary;
            public string RuntimeBoundary;
            public CaptureRecord[] Captures;
        }
    }
}
