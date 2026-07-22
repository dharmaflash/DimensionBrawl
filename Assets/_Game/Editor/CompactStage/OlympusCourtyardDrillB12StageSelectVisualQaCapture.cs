using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using DimensionBrawl.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DimensionBrawl.Editor
{
    /// <summary>
    /// Captures the admitted B1-2 Courtyard route from the real Stage Select scene.
    /// Batch invocation must omit -quit because this runner owns Play-mode exit.
    /// </summary>
    [InitializeOnLoad]
    public static class OlympusCourtyardDrillB12StageSelectVisualQaCapture
    {
        public const string ScenePath =
            "Assets/_Game/Scenes/UI/UI_StageSelect.unity";
        public const string PrefabPath =
            "Assets/_Game/UI/StageSelect/PF_UI_StageSelectScreen.prefab";
        public const string CatalogPath =
            "Assets/_Game/DesignData/UI/DB_UIStageCatalog.asset";
        public const string OutputPath =
            @"C:\tmp\DimensionBrawl-B1-2-StageSelect.png";

        private const string TrainingEntryId = "story_v1_training_route";
        private const string CourtyardEntryId = "story_v1_courtyard_drill_route";
        private const string CourtyardPlayableStageId = "OLYMPUS-COURTYARD-DRILL-01";
        private const string CourtyardScenePath =
            "Assets/_Game/Scenes/OlympusCourtyardDrillStage.unity";
        private const string CourtyardTitle = "Olympus Courtyard Drill";
        private const string CourtyardObjective =
            "Defeat the Courtyard terminal boss under Rifle Crossfire pressure.";
        private const string CorridorScenePath =
            "Assets/_Game/Scenes/OlympusCorridorInvasionStage.unity";

        private const int CaptureWidth = 1600;
        private const int CaptureHeight = 900;
        private const int InitialWarmupFrames = 12;
        private const int SelectionSettleFrames = 4;
        private const double InitialWarmupSeconds = 1.05d;
        private const double SelectionSettleSeconds = 0.4d;
        private const double TimeoutSeconds = 150d;
        private const long MinimumPngBytes = 4096L;
        private const string LogPrefix =
            "[OlympusCourtyardDrillB12StageSelectVisualQaCapture]";
        private const string SessionPrefix =
            "DimensionBrawl.B1_2.StageSelect.VisualQa.";
        private const string ActiveKey = SessionPrefix + "Active";
        private const string BatchExitKey = SessionPrefix + "BatchExit";
        private const string PhaseKey = SessionPrefix + "Phase";
        private const string FailureKey = SessionPrefix + "Failure";
        private const string StartedUtcTicksKey = SessionPrefix + "StartedUtcTicks";
        private const string BuildSettingsBeforeKey =
            SessionPrefix + "BuildSettingsBefore";
        private const string SceneHashBeforeKey = SessionPrefix + "SceneHashBefore";
        private const string PrefabHashBeforeKey = SessionPrefix + "PrefabHashBefore";
        private const string CatalogHashBeforeKey = SessionPrefix + "CatalogHashBefore";

        private static readonly string[] ProductScenePaths =
        {
            CanonicalUiBuildSettings.LoginScenePath,
            CanonicalUiBuildSettings.LobbyScenePath,
            CanonicalUiBuildSettings.StageSelectScenePath,
            CorridorScenePath,
            CourtyardScenePath,
            CanonicalUiBuildSettings.StageClearScenePath
        };

        private enum RunnerPhase
        {
            None = 0,
            RequestedPlayMode = 1,
            Capturing = 2,
            SuccessAwaitingEditMode = 3,
            FailureAwaitingEditMode = 4
        }

        private static int readyAtFrame;
        private static double readyAtEditorTime;
        private static bool courtyardSelected;
        private static bool captureAttempted;

        static OlympusCourtyardDrillB12StageSelectVisualQaCapture()
        {
            EditorApplication.update -= Tick;
            EditorApplication.update += Tick;
            EditorApplication.playModeStateChanged -= HandlePlayModeStateChanged;
            EditorApplication.playModeStateChanged += HandlePlayModeStateChanged;
        }

        [MenuItem("DimensionBrawl/B1-2/Capture Stage Select Visual QA")]
        public static void CaptureMenu()
        {
            StartCapture(exitEditorWhenFinished: false);
        }

        public static void RunBatchCapture()
        {
            StartCapture(exitEditorWhenFinished: true);
        }

        private static void StartCapture(bool exitEditorWhenFinished)
        {
            try
            {
                if (SessionState.GetBool(ActiveKey, false))
                {
                    throw new InvalidOperationException(
                        "A B1-2 Stage Select visual QA capture is already active.");
                }

                if (EditorApplication.isPlayingOrWillChangePlaymode)
                {
                    throw new InvalidOperationException(
                        "B1-2 Stage Select visual QA must start from Edit mode.");
                }

                RefuseDirtyOpenScenes();
                RequireAsset<SceneAsset>(ScenePath);
                RequireAsset<GameObject>(PrefabPath);
                UIStageCatalog catalog = RequireAsset<UIStageCatalog>(CatalogPath);
                ValidateCatalog(catalog);
                ValidateExactProductBuildSettings();

                ResetOutputFile();
                ClearSessionState();
                SessionState.SetBool(BatchExitKey, exitEditorWhenFinished);
                SessionState.SetString(BuildSettingsBeforeKey, CaptureBuildSettingsSnapshot());
                SessionState.SetString(SceneHashBeforeKey, CaptureAssetHash(ScenePath));
                SessionState.SetString(PrefabHashBeforeKey, CaptureAssetHash(PrefabPath));
                SessionState.SetString(CatalogHashBeforeKey, CaptureAssetHash(CatalogPath));
                SessionState.SetString(
                    StartedUtcTicksKey,
                    DateTime.UtcNow.Ticks.ToString(CultureInfo.InvariantCulture));
                SessionState.SetString(FailureKey, string.Empty);
                SessionState.SetInt(PhaseKey, (int)RunnerPhase.RequestedPlayMode);
                SessionState.SetBool(ActiveKey, true);
                ResetRuntimeState();

                Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                if (!scene.IsValid()
                    || !string.Equals(scene.path, ScenePath, StringComparison.Ordinal)
                    || scene.isDirty)
                {
                    throw new InvalidOperationException(
                        "UI_StageSelect did not open as the exact clean capture source.");
                }

                VerifySceneUsesExactPrefab(scene);
                Debug.Log(
                    $"{LogPrefix} Entering Play mode for {CaptureWidth}x{CaptureHeight} "
                    + "real Stage Select capture; Start remains untouched.");
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
                BeginCapturePhase();
                return;
            }

            if (state == PlayModeStateChange.EnteredEditMode)
            {
                RunnerPhase phase = ReadPhase();
                if (phase == RunnerPhase.SuccessAwaitingEditMode
                    || phase == RunnerPhase.FailureAwaitingEditMode)
                {
                    FinalizeEditorSession(
                        phase == RunnerPhase.SuccessAwaitingEditMode);
                }
            }
        }

        private static void Tick()
        {
            if (!SessionState.GetBool(ActiveKey, false))
            {
                return;
            }

            RunnerPhase phase = ReadPhase();
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
                    $"B1-2 Stage Select visual QA exceeded {TimeoutSeconds:0} seconds.");
                return;
            }

            if (phase == RunnerPhase.RequestedPlayMode && EditorApplication.isPlaying)
            {
                BeginCapturePhase();
                phase = RunnerPhase.Capturing;
            }

            if (phase != RunnerPhase.Capturing
                || !EditorApplication.isPlaying
                || Time.frameCount < readyAtFrame
                || EditorApplication.timeSinceStartup < readyAtEditorTime)
            {
                return;
            }

            try
            {
                if (!courtyardSelected)
                {
                    SelectCourtyardThroughBoundButton();
                    courtyardSelected = true;
                    readyAtFrame = Time.frameCount + SelectionSettleFrames;
                    readyAtEditorTime =
                        EditorApplication.timeSinceStartup + SelectionSettleSeconds;
                    return;
                }

                if (captureAttempted)
                {
                    return;
                }

                captureAttempted = true;
                ValidateSelectedCourtyardAndCapture();
                SessionState.SetInt(
                    PhaseKey,
                    (int)RunnerPhase.SuccessAwaitingEditMode);
                Debug.Log(
                    $"{LogPrefix} CAPTURE_PASS {CaptureWidth}x{CaptureHeight} "
                    + $"`{OutputPath}`");
                EditorApplication.ExitPlaymode();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                FinishWithFailure(exception.ToString());
            }
        }

        private static void BeginCapturePhase()
        {
            SessionState.SetInt(PhaseKey, (int)RunnerPhase.Capturing);
            ResetRuntimeState();
            readyAtFrame = Time.frameCount + InitialWarmupFrames;
            readyAtEditorTime = EditorApplication.timeSinceStartup + InitialWarmupSeconds;
        }

        private static void SelectCourtyardThroughBoundButton()
        {
            Scene scene = RequireActiveStageSelectScene();
            StageSelectScreenPresenter presenter =
                FindSingleInScene<StageSelectScreenPresenter>(scene);
            UIStageCatalog catalog = ReadObjectReference<UIStageCatalog>(
                presenter,
                "stageCatalog",
                allowNull: false);
            ValidateCatalog(catalog);
            ValidateCardShellsAndBinding(presenter, catalog, out Button courtyardButton);
            Require(string.Equals(
                    ReadSerializedString(presenter, "selectedStageId"),
                    TrainingEntryId,
                    StringComparison.Ordinal),
                "Stage Select did not begin with the accepted 01-1 backing selection id.");

            Require(presenter.HasSelectedRouteProjection,
                "Stage Select did not project its initial catalog row.");
            Require(string.Equals(
                    presenter.SelectedRouteProjection.CatalogEntryId,
                    TrainingEntryId,
                    StringComparison.Ordinal),
                "Stage Select did not begin on the accepted 01-1 training route.");
            Require(!presenter.HasAcceptedStartRequest,
                "Stage Select had already accepted Start before visual QA interaction.");
            UISceneFlowRouter router = FindSingleInScene<UISceneFlowRouter>(scene);
            Require(router.RouteRequestCount == 0,
                "A route request existed before the 01-2 selection click.");

            Require(courtyardButton.isActiveAndEnabled && courtyardButton.interactable,
                "The bound 01-2 button is not a real interactable UI control.");
            courtyardButton.onClick.Invoke();
        }

        private static void ValidateSelectedCourtyardAndCapture()
        {
            Scene scene = RequireActiveStageSelectScene();
            StageSelectScreenPresenter presenter =
                FindSingleInScene<StageSelectScreenPresenter>(scene);
            UIStageCatalog catalog = ReadObjectReference<UIStageCatalog>(
                presenter,
                "stageCatalog",
                allowNull: false);
            ValidateCatalog(catalog);
            ValidateCardShellsAndBinding(presenter, catalog, out _);

            UIStageRouteProjection projection = presenter.SelectedRouteProjection;
            Require(presenter.HasSelectedRouteProjection && projection != null,
                "The actual 01-2 click did not produce a selected route projection.");
            Require(string.Equals(projection.CatalogEntryId, CourtyardEntryId,
                    StringComparison.Ordinal),
                "The actual 01-2 click selected the wrong catalog entry.");
            Require(string.Equals(
                    ReadSerializedString(presenter, "selectedStageId"),
                    CourtyardEntryId,
                    StringComparison.Ordinal),
                "The actual 01-2 click did not retain the Courtyard backing selection id.");
            Require(projection.CatalogProjectionGeneration == 3,
                "The selected Courtyard projection is not catalog generation 3.");
            Require(string.Equals(projection.PlayableStageId, CourtyardPlayableStageId,
                    StringComparison.Ordinal),
                "The selected Courtyard projection has the wrong playable-stage id.");
            Require(string.Equals(projection.EntryScenePath, CourtyardScenePath,
                    StringComparison.Ordinal),
                "The selected Courtyard projection has the wrong scene path.");
            Require(string.Equals(projection.EntrySceneName,
                    "OlympusCourtyardDrillStage", StringComparison.Ordinal),
                "The selected Courtyard projection has the wrong scene name.");
            Require(projection.Briefing != null
                    && string.Equals(projection.Briefing.Title, CourtyardTitle,
                        StringComparison.Ordinal)
                    && string.Equals(projection.Briefing.Objective, CourtyardObjective,
                        StringComparison.Ordinal),
                "The selected Courtyard briefing is not the admitted authored briefing.");
            Require(string.IsNullOrEmpty(projection.ThreatTags)
                    && string.IsNullOrEmpty(projection.RecommendedSummonRole)
                    && string.IsNullOrEmpty(projection.RewardPreview),
                "Unsupported threat, loadout, or reward claims leaked into the projection.");
            Require(!presenter.HasAcceptedStartRequest,
                "Visual QA must not accept or dispatch the Start action.");

            Text lesson = ReadObjectReference<Text>(
                presenter,
                "combatLessonText",
                allowNull: false);
            Require(string.Equals(
                    lesson.text,
                    projection.Briefing.CombatLesson,
                    StringComparison.Ordinal)
                    && !string.IsNullOrWhiteSpace(lesson.text),
                "The Courtyard combat lesson is not the exact non-empty authored briefing.");
            RequireOptionalTextHidden(presenter, "threatTagsText", "threat tags");
            RequireOptionalTextHidden(presenter, "summonHintText", "loadout hint");
            RequireOptionalTextHidden(presenter, "rewardPreviewText", "reward preview");
            ValidateHiddenGlobalDetailClaims(presenter.transform);

            Button start = ReadObjectReference<Button>(presenter, "startButton", allowNull: false);
            Require(start.interactable,
                "The admitted Courtyard route did not leave Start available for the player.");
            UISceneFlowRouter router = FindSingleInScene<UISceneFlowRouter>(scene);
            Require(router.RouteRequestCount == 0,
                "Selecting 01-2 dispatched a route even though Start was never clicked.");

            Camera camera = FindSingleActiveInScene<Camera>(scene);
            Canvas[] canvases = FindAllInScene<Canvas>(scene);
            Require(canvases.Length > 0, "UI_StageSelect has no Canvas to capture.");
            RenderCameraAndCanvases(camera, canvases, presenter);
        }

        private static void ValidateCatalog(UIStageCatalog catalog)
        {
            Require(catalog != null, "DB_UIStageCatalog is missing.");
            Require(catalog.ProjectionSchemaVersion
                    == UIStageCatalog.SupportedProjectionSchemaVersion,
                "DB_UIStageCatalog has the wrong projection schema.");
            Require(catalog.CatalogProjectionGeneration == 3,
                "B1-2 visual QA requires catalog generation 3.");
            Require(catalog.StageCount == 2,
                "B1-2 visual QA requires exactly two admitted catalog rows.");
            Require(catalog.TryValidateEntryIdentities(out UIStageRouteProjectionRejectReason reject),
                $"DB_UIStageCatalog identities are invalid: {reject}.");

            UIStageCatalog.StageEntry training = catalog.GetStage(0);
            UIStageCatalog.StageEntry courtyard = catalog.GetStage(1);
            Require(string.Equals(training.Id, TrainingEntryId, StringComparison.Ordinal),
                "Catalog row 0 is not the accepted training route.");
            Require(string.Equals(courtyard.Id, CourtyardEntryId, StringComparison.Ordinal),
                "Catalog row 1 is not the Courtyard route.");
            Require(string.Equals(courtyard.DisplayName, CourtyardTitle, StringComparison.Ordinal)
                    && string.Equals(courtyard.Summary, CourtyardObjective,
                        StringComparison.Ordinal),
                "Courtyard catalog presentation does not mirror the authored briefing.");
            Require(string.IsNullOrEmpty(courtyard.ThreatTags)
                    && string.IsNullOrEmpty(courtyard.RecommendedSummonRole)
                    && string.IsNullOrEmpty(courtyard.MockRewardPreview),
                "Courtyard catalog contains unsupported presentation claims.");
        }

        private static void ValidateCardShellsAndBinding(
            StageSelectScreenPresenter presenter,
            UIStageCatalog catalog,
            out Button courtyardButton)
        {
            Transform card01 = FindUniqueDescendant(presenter.transform, "01-1_StageCard");
            Transform card02 = FindUniqueDescendant(presenter.transform, "01-2_StageCard");
            Transform card03 = FindUniqueDescendant(presenter.transform, "01-3_StageCard");
            Transform card04 = FindUniqueDescendant(presenter.transform, "01-4_StageCard");
            Require(card01.gameObject.activeSelf && card01.gameObject.activeInHierarchy,
                "The admitted 01-1 card is not active.");
            Require(card02.gameObject.activeSelf && card02.gameObject.activeInHierarchy,
                "The admitted 01-2 card is not active.");
            Require(!card03.gameObject.activeSelf && !card04.gameObject.activeSelf,
                "Unbound 01-3/01-4 placeholder cards must remain inactive.");

            SerializedObject serializedPresenter = new SerializedObject(presenter);
            serializedPresenter.UpdateIfRequiredOrScript();
            SerializedProperty entries = RequireProperty(
                serializedPresenter,
                "stageFocusEntries");
            Require(entries.isArray && entries.arraySize == 2,
                "Stage Select must contain exactly two focus bindings.");
            Button trainingButton = null;
            courtyardButton = null;
            for (int i = 0; i < entries.arraySize; i++)
            {
                SerializedProperty entry = entries.GetArrayElementAtIndex(i);
                string expectedId = catalog.GetStage(i).Id;
                string actualId = RequireRelativeProperty(entry, "stageId").stringValue;
                Button button = RequireRelativeProperty(entry, "selectionButton")
                    .objectReferenceValue as Button;
                RectTransform target = RequireRelativeProperty(entry, "stageTarget")
                    .objectReferenceValue as RectTransform;
                Require(string.Equals(actualId, expectedId, StringComparison.Ordinal)
                        && button != null
                        && target != null
                        && ReferenceEquals(button.transform, target),
                    $"Stage focus binding {i} does not exactly bind catalog row {i}.");
                if (i == 0)
                {
                    trainingButton = button;
                }
                else if (i == 1)
                {
                    courtyardButton = button;
                }
            }

            Require(trainingButton != null
                    && ReferenceEquals(trainingButton.transform, card01),
                "Catalog row 0 is not bound to the actual 01-1 card button.");
            Require(courtyardButton != null
                    && ReferenceEquals(courtyardButton.transform, card02),
                "Catalog row 1 is not bound to the actual 01-2 card button.");
            ValidateTruthfulCard(card01, "01-1", catalog.GetStage(0).DisplayName);
            ValidateTruthfulCard(card02, "01-2", CourtyardTitle);
            ValidateTruthfulChapterInventory(presenter.transform);
            ValidateExactRouteGateMembership(presenter, trainingButton, courtyardButton);
        }

        private static void ValidateExactRouteGateMembership(
            StageSelectScreenPresenter presenter,
            Button trainingButton,
            Button courtyardButton)
        {
            UIRouteInteractableGate[] gates =
                presenter.GetComponentsInChildren<UIRouteInteractableGate>(true);
            Require(gates.Length == 1,
                $"Stage Select requires exactly one route interactable gate, but found {gates.Length}.");

            var expected = new HashSet<Selectable>
            {
                ReadObjectReference<Button>(presenter, "backButton", allowNull: false),
                ReadObjectReference<Button>(presenter, "startButton", allowNull: false),
                trainingButton,
                courtyardButton
            };
            Require(expected.Count == 4,
                "Stage Select route gate product controls are missing or duplicated.");

            SerializedObject serializedGate = new SerializedObject(gates[0]);
            serializedGate.UpdateIfRequiredOrScript();
            SerializedProperty selectables = RequireProperty(serializedGate, "selectables");
            Require(selectables.isArray && selectables.arraySize == expected.Count,
                "Stage Select route gate must bind exactly Back, Start, 01-1, and 01-2.");
            for (int i = 0; i < selectables.arraySize; i++)
            {
                Selectable selectable =
                    selectables.GetArrayElementAtIndex(i).objectReferenceValue as Selectable;
                Require(selectable != null && expected.Remove(selectable),
                    "Stage Select route gate contains a missing, duplicate, or non-product control.");
            }

            Require(expected.Count == 0,
                "Stage Select route gate is missing an admitted product control.");
        }

        private static void ValidateTruthfulChapterInventory(Transform root)
        {
            Transform selected = FindUniqueDescendant(root, "EP 01_SelectedChapterCard");
            Button selectedButton = selected.GetComponent<Button>();
            Text episode = FindUniqueDescendant(selected, "EpisodeText").GetComponent<Text>();
            Text title = FindUniqueDescendant(selected, "TitleText").GetComponent<Text>();
            Text percent = FindUniqueDescendant(selected, "PercentText").GetComponent<Text>();
            Require(selected.gameObject.activeSelf && selected.gameObject.activeInHierarchy,
                "The admitted EP 01 chapter card is not active in the runtime hierarchy.");
            Require(selectedButton != null,
                "The admitted EP 01 chapter card has no Button component at runtime.");
            Require(selectedButton.enabled,
                "The admitted EP 01 chapter card Button component was disabled at runtime.");
            Require(!selectedButton.interactable,
                "The admitted EP 01 chapter card exposes a dead interactive Button at runtime.");
            Require(selectedButton.targetGraphic != null,
                "The admitted EP 01 chapter card Button lost its target Graphic at runtime.");
            Require(!selectedButton.targetGraphic.raycastTarget,
                "The admitted EP 01 chapter card target Graphic still blocks pointer raycasts.");
            Require(episode != null
                    && episode.gameObject.activeSelf
                    && string.Equals(episode.text, "EP 01", StringComparison.Ordinal),
                $"The admitted chapter number is not exactly 'EP 01' at runtime (actual='{episode?.text ?? "<null>"}').");
            Require(title != null
                    && title.gameObject.activeSelf
                    && string.Equals(title.text, "차원 안정화", StringComparison.Ordinal),
                $"The admitted chapter title is not exactly '차원 안정화' at runtime (actual='{title?.text ?? "<null>"}').");
            Require(percent != null
                    && !percent.gameObject.activeSelf
                    && string.IsNullOrEmpty(percent.text),
                $"The admitted EP 01 chapter card exposes unsupported progress at runtime (active={percent?.gameObject.activeSelf}, text='{percent?.text ?? "<null>"}').");

            string[] placeholders =
            {
                "EP 02_ChapterCard",
                "EP 03_ChapterCard",
                "EP 04_ChapterCard"
            };
            for (int i = 0; i < placeholders.Length; i++)
            {
                Transform placeholder = FindUniqueDescendant(root, placeholders[i]);
                Button button = placeholder.GetComponent<Button>();
                CanvasGroup canvasGroup = placeholder.GetComponent<CanvasGroup>();
                Require(!placeholder.gameObject.activeSelf
                        && button != null
                        && !button.interactable
                        && (button.targetGraphic == null
                            || !button.targetGraphic.raycastTarget)
                        && (canvasGroup == null
                            || (!canvasGroup.interactable
                                && !canvasGroup.blocksRaycasts)),
                    $"Unadmitted chapter placeholder '{placeholders[i]}' is visible or interactive.");
            }
        }

        private static void ValidateTruthfulCard(
            Transform card,
            string expectedNumber,
            string expectedTitle)
        {
            Transform lockIcon = FindOptionalUniqueDescendant(card, "LockIcon");
            Require(lockIcon == null || !lockIcon.gameObject.activeSelf,
                $"{card.name} displays a false lock state.");
            Transform stagePercent = FindUniqueDescendant(card, "StagePercentText");
            Text stagePercentText = stagePercent.GetComponent<Text>();
            Require(!stagePercent.gameObject.activeSelf
                    && stagePercentText != null
                    && string.IsNullOrEmpty(stagePercentText.text),
                $"{card.name} displays or stores unsupported completion progress.");
            for (int star = 1; star <= 3; star++)
            {
                Require(!FindUniqueDescendant(card, $"Star{star}").gameObject.activeSelf,
                    $"{card.name} displays unsupported star progression.");
            }

            Text number = FindUniqueDescendant(card, "StageNumberText").GetComponent<Text>();
            Text title = FindUniqueDescendant(card, "StageTitleText").GetComponent<Text>();
            Require(number != null && string.Equals(number.text, expectedNumber,
                    StringComparison.Ordinal),
                $"{card.name} has the wrong stage number.");
            Require(title != null && string.Equals(title.text, expectedTitle,
                    StringComparison.Ordinal),
                $"{card.name} has the wrong stage title.");
        }

        private static void RequireVisibleExactText(Text text, string expected, string label)
        {
            Require(text != null
                    && text.isActiveAndEnabled
                    && text.gameObject.activeInHierarchy
                    && string.Equals(text.text, expected, StringComparison.Ordinal)
                    && text.color.a > 0.01f
                    && text.canvasRenderer.GetAlpha() > 0.01f
                    && !text.canvasRenderer.cull,
                $"The {label} is not visibly rendered as the exact admitted value.");
        }

        private static void RequireOptionalTextHidden(
            StageSelectScreenPresenter presenter,
            string propertyName,
            string label)
        {
            Text text = ReadObjectReference<Text>(presenter, propertyName, allowNull: true);
            if (text == null)
            {
                return;
            }

            Require(string.IsNullOrEmpty(text.text) && !text.gameObject.activeSelf,
                $"Unsupported {label} is visible in Stage Select.");
        }

        private static void ValidateHiddenGlobalDetailClaims(Transform root)
        {
            RequireHiddenGlobalDetailClaim(
                root,
                "ChapterProgressLabel",
                requireEmptyText: true);
            RequireHiddenGlobalDetailClaim(
                root,
                "ChapterPercentText",
                requireEmptyText: true);
            RequireHiddenGlobalDetailClaim(
                root,
                "ChapterProgress",
                requireEmptyText: false);
            RequireHiddenGlobalDetailClaim(
                root,
                "ChapterProgressBackground",
                requireEmptyText: false);
            RequireHiddenGlobalDetailClaim(
                root,
                "SummaryFrame",
                requireEmptyText: false);
            RequireHiddenGlobalDetailClaim(
                root,
                "SummaryText",
                requireEmptyText: true);
        }

        private static void RequireHiddenGlobalDetailClaim(
            Transform root,
            string objectName,
            bool requireEmptyText)
        {
            Transform target = FindUniqueDescendant(root, objectName);
            Text text = target.GetComponent<Text>();
            Require(
                !target.gameObject.activeSelf
                    && (!requireEmptyText
                        || (text != null && string.IsNullOrEmpty(text.text))),
                $"Stage Select '{objectName}' must be inactive"
                + (requireEmptyText ? " and have empty text." : "."));
        }

        private static void ValidateRenderedDetailPanel(
            StageSelectScreenPresenter presenter,
            Camera camera)
        {
            Text title = ReadObjectReference<Text>(presenter, "stageNameText", allowNull: false);
            Text objective = ReadObjectReference<Text>(presenter, "summaryText", allowNull: false);
            Text lesson = ReadObjectReference<Text>(
                presenter,
                "combatLessonText",
                allowNull: false);

            RequireVisibleExactText(title, CourtyardTitle, "rendered stage title");
            RequireVisibleExactText(objective, CourtyardObjective, "rendered objective");
            RequireVisibleExactText(lesson, lesson.text, "rendered combat lesson");
            RequireTextFullyRendered(title, CourtyardTitle, "stage title");
            RequireTextFullyRendered(objective, CourtyardObjective, "objective");
            RequireTextFullyRendered(lesson, lesson.text, "combat lesson");
            RequireVerticallySeparated(
                title.rectTransform,
                objective.rectTransform,
                camera,
                "stage title",
                "objective");
            RequireVerticallySeparated(
                objective.rectTransform,
                lesson.rectTransform,
                camera,
                "objective",
                "combat lesson");

            Button start = ReadObjectReference<Button>(presenter, "startButton", allowNull: false);
            CanvasGroup startCanvasGroup = start.GetComponent<CanvasGroup>();
            Transform startFrame = FindUniqueDescendant(start.transform, "Frame");
            Graphic startFrameGraphic = startFrame.GetComponent<Graphic>();
            Text startLabel = FindUniqueDescendant(start.transform, "StageStartText")
                .GetComponent<Text>();
            Require(start.isActiveAndEnabled
                    && start.interactable
                    && startCanvasGroup != null
                    && startCanvasGroup.alpha >= 0.99f
                    && startFrame.gameObject.activeInHierarchy
                    && startFrameGraphic != null
                    && startFrameGraphic.color.a > 0.01f
                    && startFrameGraphic.canvasRenderer.GetAlpha() > 0.01f,
                "The Start button is interactable but not visibly rendered after its entrance motion.");
            RequireVisibleExactText(startLabel, "작전 시작", "rendered Start label");
            RequireTextFullyRendered(startLabel, "작전 시작", "Start label");
            Require(!ScreenRectsOverlap(
                    lesson.rectTransform,
                    start.GetComponent<RectTransform>(),
                    camera),
                "The rendered combat lesson overlaps the Start button.");
            RequireRectInsideCapture(title.rectTransform, camera, "stage title");
            RequireRectInsideCapture(objective.rectTransform, camera, "objective");
            RequireRectInsideCapture(lesson.rectTransform, camera, "combat lesson");
            RequireRectInsideCapture(
                start.GetComponent<RectTransform>(),
                camera,
                "Start button");
            ValidateRenderedChapterAndStageCards(presenter, camera);
        }

        private static void ValidateRenderedChapterAndStageCards(
            StageSelectScreenPresenter presenter,
            Camera camera)
        {
            Transform chapter = FindUniqueDescendant(
                presenter.transform,
                "EP 01_SelectedChapterCard");
            Text episode = FindUniqueDescendant(chapter, "EpisodeText").GetComponent<Text>();
            Text chapterTitle = FindUniqueDescendant(chapter, "TitleText").GetComponent<Text>();
            RequireVisibleExactText(episode, "EP 01", "rendered chapter number");
            RequireVisibleExactText(chapterTitle, "차원 안정화", "rendered chapter title");
            RequireTextFullyRendered(episode, "EP 01", "chapter number");
            RequireTextFullyRendered(chapterTitle, "차원 안정화", "chapter title");
            RequireRectInsideCapture(episode.rectTransform, camera, "chapter number");
            RequireRectInsideCapture(chapterTitle.rectTransform, camera, "chapter title");

            string[] cardNames = { "01-1_StageCard", "01-2_StageCard" };
            string[] cardNumbers = { "01-1", "01-2" };
            UIStageCatalog catalog = ReadObjectReference<UIStageCatalog>(
                presenter,
                "stageCatalog",
                allowNull: false);
            string[] cardTitles =
            {
                catalog.GetStage(0).DisplayName,
                CourtyardTitle
            };
            for (int i = 0; i < cardNames.Length; i++)
            {
                Transform card = FindUniqueDescendant(presenter.transform, cardNames[i]);
                Text number = FindUniqueDescendant(card, "StageNumberText").GetComponent<Text>();
                Text cardTitle = FindUniqueDescendant(card, "StageTitleText").GetComponent<Text>();
                RequireVisibleExactText(number, cardNumbers[i], $"rendered {cardNumbers[i]} number");
                RequireVisibleExactText(cardTitle, cardTitles[i], $"rendered {cardNumbers[i]} title");
                RequireTextFullyRendered(number, cardNumbers[i], $"{cardNumbers[i]} number");
                RequireTextFullyRendered(cardTitle, cardTitles[i], $"{cardNumbers[i]} title");
                RequireRectInsideCapture(number.rectTransform, camera, $"{cardNumbers[i]} number");
                RequireRectInsideCapture(cardTitle.rectTransform, camera, $"{cardNumbers[i]} title");
            }
        }

        private static void RequireTextFullyRendered(
            Text text,
            string expected,
            string label)
        {
            Require(text.rectTransform.rect.width > 1f && text.rectTransform.rect.height > 1f,
                $"The rendered {label} has no usable layout area.");
            TextGenerationSettings settings = text.GetGenerationSettings(
                text.rectTransform.rect.size);
            var generator = new TextGenerator(Mathf.Max(8, expected.Length + 1));
            bool populated = generator.Populate(expected, settings);
            Require(populated && generator.characterCountVisible >= expected.Length,
                $"The rendered {label} is truncated "
                + $"({generator.characterCountVisible}/{expected.Length} characters visible).");
        }

        private static void RequireVerticallySeparated(
            RectTransform upper,
            RectTransform lower,
            Camera camera,
            string upperLabel,
            string lowerLabel)
        {
            Rect upperBounds = GetScreenRect(upper, camera);
            Rect lowerBounds = GetScreenRect(lower, camera);
            Require(upperBounds.yMin >= lowerBounds.yMax + 0.5f,
                $"The rendered {upperLabel} overlaps the {lowerLabel}.");
        }

        private static bool ScreenRectsOverlap(
            RectTransform first,
            RectTransform second,
            Camera camera)
        {
            if (first == null || second == null)
            {
                return true;
            }

            Rect firstBounds = GetScreenRect(first, camera);
            Rect secondBounds = GetScreenRect(second, camera);
            return firstBounds.xMin < secondBounds.xMax
                && firstBounds.xMax > secondBounds.xMin
                && firstBounds.yMin < secondBounds.yMax
                && firstBounds.yMax > secondBounds.yMin;
        }

        private static void RequireRectInsideCapture(
            RectTransform rect,
            Camera camera,
            string label)
        {
            Rect bounds = GetScreenRect(rect, camera);
            Require(bounds.xMin >= -0.5f
                    && bounds.xMax <= CaptureWidth + 0.5f
                    && bounds.yMin >= -0.5f
                    && bounds.yMax <= CaptureHeight + 0.5f,
                $"The rendered {label} extends outside the 1600x900 capture.");
        }

        private static Rect GetScreenRect(RectTransform rect, Camera camera)
        {
            Vector3[] corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            Vector2 first = RectTransformUtility.WorldToScreenPoint(camera, corners[0]);
            float minimumX = first.x;
            float maximumX = first.x;
            float minimumY = first.y;
            float maximumY = first.y;
            for (int i = 1; i < corners.Length; i++)
            {
                Vector2 screen = RectTransformUtility.WorldToScreenPoint(camera, corners[i]);
                minimumX = Mathf.Min(minimumX, screen.x);
                maximumX = Mathf.Max(maximumX, screen.x);
                minimumY = Mathf.Min(minimumY, screen.y);
                maximumY = Mathf.Max(maximumY, screen.y);
            }

            return Rect.MinMaxRect(minimumX, minimumY, maximumX, maximumY);
        }

        private static void RenderCameraAndCanvases(
            Camera camera,
            Canvas[] canvases,
            StageSelectScreenPresenter presenter)
        {
            RenderTexture previousTarget = camera.targetTexture;
            RenderTexture previousActive = RenderTexture.active;
            float previousAspect = camera.aspect;
            float previousTimeScale = Time.timeScale;
            var canvasStates = new List<CanvasCaptureState>(canvases.Length);
            var target = new RenderTexture(
                CaptureWidth,
                CaptureHeight,
                24,
                RenderTextureFormat.ARGB32,
                RenderTextureReadWrite.sRGB)
            {
                name = "DimensionBrawl_B1_2_StageSelect_VisualQA",
                antiAliasing = 1,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            var image = new Texture2D(
                CaptureWidth,
                CaptureHeight,
                TextureFormat.RGBA32,
                mipChain: false,
                linear: false);

            try
            {
                Time.timeScale = 0f;
                target.Create();
                Require(target.IsCreated(),
                    "Failed to create the B1-2 Stage Select RenderTexture.");
                camera.targetTexture = target;
                camera.aspect = CaptureWidth / (float)CaptureHeight;

                for (int i = 0; i < canvases.Length; i++)
                {
                    Canvas canvas = canvases[i];
                    if (canvas == null)
                    {
                        continue;
                    }

                    var state = new CanvasCaptureState(canvas);
                    canvasStates.Add(state);
                    if (canvas.renderMode == RenderMode.ScreenSpaceOverlay
                        || canvas.renderMode == RenderMode.ScreenSpaceCamera)
                    {
                        canvas.renderMode = RenderMode.ScreenSpaceCamera;
                        canvas.worldCamera = camera;
                        canvas.planeDistance = Mathf.Max(camera.nearClipPlane + 0.1f, 0.5f);
                        ApplyExactResolutionScale(canvas, state.Scaler);
                    }
                }

                Canvas.ForceUpdateCanvases();
                camera.Render();
                RefreshRectMaskClipping(presenter);
                Canvas.ForceUpdateCanvases();
                ValidateRenderedDetailPanel(presenter, camera);
                camera.Render();
                RenderTexture.active = target;
                image.ReadPixels(
                    new Rect(0f, 0f, CaptureWidth, CaptureHeight),
                    0,
                    0,
                    recalculateMipMaps: false);
                image.Apply(updateMipmaps: false, makeNoLongerReadable: false);

                PixelAudit audit = AuditPixels(image);
                Require(audit.IsUsable,
                    "B1-2 Stage Select capture is blank or lacks visual range "
                    + $"(mean={audit.MeanLuminance:0.0000}, "
                    + $"range={audit.LuminanceRange:0.0000}).");
                byte[] png = image.EncodeToPNG();
                Require(IsValidPngPayload(png),
                    "B1-2 Stage Select produced an invalid PNG payload.");

                string directory = Path.GetDirectoryName(OutputPath);
                Require(!string.IsNullOrWhiteSpace(directory),
                    "B1-2 Stage Select output directory is invalid.");
                Directory.CreateDirectory(directory);
                File.WriteAllBytes(OutputPath, png);
                var output = new FileInfo(OutputPath);
                Require(output.Exists && output.Length == png.LongLength,
                    "B1-2 Stage Select PNG was not written completely.");
            }
            finally
            {
                for (int i = canvasStates.Count - 1; i >= 0; i--)
                {
                    canvasStates[i].Restore();
                }

                camera.targetTexture = previousTarget;
                camera.aspect = previousAspect;
                RenderTexture.active = previousActive;
                Time.timeScale = previousTimeScale;
                if (target.IsCreated())
                {
                    target.Release();
                }

                UnityEngine.Object.DestroyImmediate(image);
                UnityEngine.Object.DestroyImmediate(target);
                Canvas.ForceUpdateCanvases();
            }
        }

        private static void RefreshRectMaskClipping(StageSelectScreenPresenter presenter)
        {
            RectMask2D[] masks = presenter.GetComponentsInChildren<RectMask2D>(true);
            for (int i = 0; i < masks.Length; i++)
            {
                RectMask2D mask = masks[i];
                if (mask != null && mask.isActiveAndEnabled)
                {
                    mask.PerformClipping();
                }
            }
        }

        private static void ApplyExactResolutionScale(Canvas canvas, CanvasScaler scaler)
        {
            if (scaler == null
                || scaler.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize)
            {
                return;
            }

            Vector2 reference = scaler.referenceResolution;
            float logWidth = Mathf.Log(CaptureWidth / Mathf.Max(1f, reference.x), 2f);
            float logHeight = Mathf.Log(CaptureHeight / Mathf.Max(1f, reference.y), 2f);
            float logScale = scaler.screenMatchMode switch
            {
                CanvasScaler.ScreenMatchMode.Expand => Mathf.Min(logWidth, logHeight),
                CanvasScaler.ScreenMatchMode.Shrink => Mathf.Max(logWidth, logHeight),
                _ => Mathf.Lerp(logWidth, logHeight, scaler.matchWidthOrHeight)
            };
            scaler.enabled = false;
            canvas.scaleFactor = Mathf.Pow(2f, logScale);
        }

        private static PixelAudit AuditPixels(Texture2D image)
        {
            Color32[] pixels = image.GetPixels32();
            if (pixels == null || pixels.Length == 0)
            {
                return default;
            }

            float minimum = 1f;
            float maximum = 0f;
            double total = 0d;
            int count = 0;
            int stride = Mathf.Max(1, pixels.Length / 18000);
            for (int i = 0; i < pixels.Length; i += stride)
            {
                Color32 pixel = pixels[i];
                float luminance = ((pixel.r / 255f) * 0.2126f)
                    + ((pixel.g / 255f) * 0.7152f)
                    + ((pixel.b / 255f) * 0.0722f);
                minimum = Mathf.Min(minimum, luminance);
                maximum = Mathf.Max(maximum, luminance);
                total += luminance;
                count++;
            }

            float mean = count > 0 ? (float)(total / count) : 0f;
            float range = Mathf.Max(0f, maximum - minimum);
            return new PixelAudit(
                mean,
                range,
                maximum > 0.045f && range > 0.018f && mean > 0.006f);
        }

        private static void FinalizeEditorSession(bool capturePhasePassed)
        {
            bool exitEditor = SessionState.GetBool(BatchExitKey, false);
            var issues = new List<string>();
            string failure = SessionState.GetString(FailureKey, string.Empty);
            if (!string.IsNullOrWhiteSpace(failure))
            {
                issues.Add(failure.Trim());
            }

            if (!string.Equals(SessionState.GetString(BuildSettingsBeforeKey, string.Empty),
                    CaptureBuildSettingsSnapshot(), StringComparison.Ordinal))
            {
                issues.Add("Editor Build Settings changed during B1-2 Stage Select capture.");
            }

            TryAddValidationIssue(issues, ValidateExactProductBuildSettings);
            CompareAssetHash(issues, SceneHashBeforeKey, ScenePath, "UI_StageSelect");
            CompareAssetHash(issues, PrefabHashBeforeKey, PrefabPath,
                "PF_UI_StageSelectScreen");
            CompareAssetHash(issues, CatalogHashBeforeKey, CatalogPath,
                "DB_UIStageCatalog");

            Scene activeScene = SceneManager.GetActiveScene();
            if (activeScene.IsValid()
                && string.Equals(activeScene.path, ScenePath, StringComparison.Ordinal)
                && activeScene.isDirty)
            {
                issues.Add("UI_StageSelect is dirty after B1-2 visual QA.");
            }

            TryAddValidationIssue(issues, ValidateOutputFile);
            bool success = capturePhasePassed && issues.Count == 0;
            if (!success)
            {
                ResetOutputFile();
            }

            string finalFailure = success ? string.Empty : string.Join("\n", issues);
            ClearSessionState();
            if (success)
            {
                Debug.Log(
                    $"{LogPrefix} BATCH_CAPTURE_PASS {CaptureWidth}x{CaptureHeight} "
                    + $"`{OutputPath}`; exact six-row Build Settings unchanged.");
            }
            else
            {
                Debug.LogError($"{LogPrefix} BATCH_CAPTURE_FAIL\n{finalFailure}");
            }

            if (exitEditor)
            {
                EditorApplication.Exit(success ? 0 : 1);
            }
        }

        private static void FinishWithFailure(string failure)
        {
            if (ReadPhase() == RunnerPhase.FailureAwaitingEditMode)
            {
                return;
            }

            SessionState.SetString(
                FailureKey,
                string.IsNullOrWhiteSpace(failure)
                    ? "Unknown B1-2 Stage Select visual QA failure."
                    : failure);
            SessionState.SetInt(PhaseKey, (int)RunnerPhase.FailureAwaitingEditMode);
            ResetOutputFile();
            Debug.LogError($"{LogPrefix} BATCH_CAPTURE_FAIL\n{failure}");
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorApplication.ExitPlaymode();
                return;
            }

            FinalizeEditorSession(capturePhasePassed: false);
        }

        private static void HandleLaunchFailure(Exception exception, bool exitEditor)
        {
            Debug.LogException(exception);
            ResetOutputFile();
            ClearSessionState();
            Debug.LogError($"{LogPrefix} BATCH_CAPTURE_FAIL\n{exception}");
            if (exitEditor)
            {
                EditorApplication.Exit(1);
            }
        }

        private static void ValidateExactProductBuildSettings()
        {
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            Require(scenes.Length == ProductScenePaths.Length,
                $"Product Build Settings require exactly {ProductScenePaths.Length} rows; "
                + $"found {scenes.Length}.");
            for (int i = 0; i < ProductScenePaths.Length; i++)
            {
                Require(scenes[i].enabled
                        && string.Equals(scenes[i].path, ProductScenePaths[i],
                            StringComparison.Ordinal),
                    $"Build Settings row {i} is not the exact enabled product scene "
                    + $"`{ProductScenePaths[i]}`.");
            }
        }

        private static void ValidateOutputFile()
        {
            Require(File.Exists(OutputPath), "The exact B1-2 Stage Select PNG is missing.");
            byte[] png = File.ReadAllBytes(OutputPath);
            Require(IsValidPngPayload(png),
                "The exact B1-2 Stage Select PNG is incomplete or invalid.");
            var decoded = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            try
            {
                Require(ImageConversion.LoadImage(decoded, png, markNonReadable: false)
                        && decoded.width == CaptureWidth
                        && decoded.height == CaptureHeight,
                    $"The B1-2 Stage Select PNG is not exactly "
                    + $"{CaptureWidth}x{CaptureHeight}.");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(decoded);
            }
        }

        private static bool IsValidPngPayload(byte[] png)
        {
            return png != null
                && png.LongLength >= MinimumPngBytes
                && png.Length >= 8
                && png[0] == 0x89
                && png[1] == 0x50
                && png[2] == 0x4E
                && png[3] == 0x47
                && png[4] == 0x0D
                && png[5] == 0x0A
                && png[6] == 0x1A
                && png[7] == 0x0A;
        }

        private static string CaptureBuildSettingsSnapshot()
        {
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            var snapshot = new StringBuilder();
            snapshot.Append(scenes.Length).Append('|');
            for (int i = 0; i < scenes.Length; i++)
            {
                string path = scenes[i].path ?? string.Empty;
                snapshot.Append(scenes[i].enabled ? '1' : '0')
                    .Append(':')
                    .Append(path.Length)
                    .Append(':')
                    .Append(path)
                    .Append('|');
            }

            return snapshot.ToString();
        }

        private static void CompareAssetHash(
            ICollection<string> issues,
            string key,
            string path,
            string label)
        {
            string before = SessionState.GetString(key, string.Empty);
            string after = CaptureAssetHash(path);
            if (string.IsNullOrWhiteSpace(before)
                || !string.Equals(before, after, StringComparison.Ordinal))
            {
                issues.Add($"{label} or one of its dependencies changed during capture.");
            }
        }

        private static string CaptureAssetHash(string path)
        {
            return AssetDatabase.GetAssetDependencyHash(path).ToString();
        }

        private static void VerifySceneUsesExactPrefab(Scene scene)
        {
            StageSelectScreenPresenter presenter =
                FindSingleInScene<StageSelectScreenPresenter>(scene);
            StageSelectScreenPresenter source =
                PrefabUtility.GetCorrespondingObjectFromSource(presenter);
            Require(source != null
                    && string.Equals(AssetDatabase.GetAssetPath(source), PrefabPath,
                        StringComparison.Ordinal),
                "UI_StageSelect presenter is not an instance of the exact product prefab.");
        }

        private static Scene RequireActiveStageSelectScene()
        {
            Scene scene = SceneManager.GetActiveScene();
            Require(scene.IsValid()
                    && string.Equals(scene.path, ScenePath, StringComparison.Ordinal),
                $"Active Play-mode scene is `{scene.path}`, expected `{ScenePath}`.");
            return scene;
        }

        private static Transform FindUniqueDescendant(Transform root, string name)
        {
            Transform result = FindOptionalUniqueDescendant(root, name);
            Require(result != null, $"Missing descendant `{name}` under `{root.name}`.");
            return result;
        }

        private static Transform FindOptionalUniqueDescendant(Transform root, string name)
        {
            Transform result = null;
            Transform[] descendants = root.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < descendants.Length; i++)
            {
                if (!string.Equals(descendants[i].name, name, StringComparison.Ordinal))
                {
                    continue;
                }

                Require(result == null, $"Duplicate descendant `{name}` under `{root.name}`.");
                result = descendants[i];
            }
            return result;
        }

        private static string ReadSerializedString(
            UnityEngine.Object target,
            string propertyName)
        {
            var serialized = new SerializedObject(target);
            serialized.UpdateIfRequiredOrScript();
            SerializedProperty property = RequireProperty(serialized, propertyName);
            Require(property.propertyType == SerializedPropertyType.String,
                $"{target.GetType().Name}.{propertyName} is not a serialized string.");
            return property.stringValue;
        }

        private static T ReadObjectReference<T>(
            UnityEngine.Object target,
            string propertyName,
            bool allowNull)
            where T : UnityEngine.Object
        {
            var serialized = new SerializedObject(target);
            serialized.UpdateIfRequiredOrScript();
            SerializedProperty property = RequireProperty(serialized, propertyName);
            T value = property.objectReferenceValue as T;
            if (!allowNull)
            {
                Require(value != null,
                    $"{target.GetType().Name}.{propertyName} is missing or has the wrong type.");
            }

            return value;
        }

        private static SerializedProperty RequireProperty(
            SerializedObject serialized,
            string propertyName)
        {
            SerializedProperty property = serialized.FindProperty(propertyName);
            Require(property != null,
                $"Missing serialized property `{propertyName}` on "
                + $"{serialized.targetObject.GetType().Name}.");
            return property;
        }

        private static SerializedProperty RequireRelativeProperty(
            SerializedProperty parent,
            string propertyName)
        {
            SerializedProperty property = parent.FindPropertyRelative(propertyName);
            Require(property != null,
                $"Missing serialized relative property `{propertyName}`.");
            return property;
        }

        private static T RequireAsset<T>(string path) where T : UnityEngine.Object
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            Require(asset != null, $"Missing required asset `{path}`.");
            return asset;
        }

        private static T FindSingleInScene<T>(Scene scene) where T : Component
        {
            T[] values = FindAllInScene<T>(scene);
            Require(values.Length == 1,
                $"Scene `{scene.path}` requires exactly one {typeof(T).Name}; "
                + $"found {values.Length}.");
            return values[0];
        }

        private static T FindSingleActiveInScene<T>(Scene scene) where T : Behaviour
        {
            T[] values = FindAllInScene<T>(scene);
            T result = null;
            int activeCount = 0;
            for (int i = 0; i < values.Length; i++)
            {
                if (!values[i].isActiveAndEnabled)
                {
                    continue;
                }

                activeCount++;
                result = values[i];
            }

            Require(activeCount == 1,
                $"Scene `{scene.path}` requires exactly one active {typeof(T).Name}; "
                + $"found {activeCount}.");
            return result;
        }

        private static T[] FindAllInScene<T>(Scene scene) where T : Component
        {
            var values = new List<T>();
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                values.AddRange(root.GetComponentsInChildren<T>(includeInactive: true));
            }

            return values.ToArray();
        }

        private static void RefuseDirtyOpenScenes()
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.isDirty)
                {
                    throw new InvalidOperationException(
                        $"Refusing to replace dirty scene `{scene.path}` for visual QA.");
                }
            }
        }

        private static void TryAddValidationIssue(
            ICollection<string> issues,
            Action validation)
        {
            try
            {
                validation();
            }
            catch (Exception exception)
            {
                issues.Add(exception.Message);
            }
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }

        private static bool HasTimedOut()
        {
            string ticksText = SessionState.GetString(StartedUtcTicksKey, string.Empty);
            if (!long.TryParse(ticksText, NumberStyles.Integer, CultureInfo.InvariantCulture,
                    out long ticks)
                || ticks <= 0)
            {
                return true;
            }

            return (DateTime.UtcNow - new DateTime(ticks, DateTimeKind.Utc)).TotalSeconds
                > TimeoutSeconds;
        }

        private static RunnerPhase ReadPhase()
        {
            return (RunnerPhase)SessionState.GetInt(PhaseKey, (int)RunnerPhase.None);
        }

        private static void ResetOutputFile()
        {
            if (File.Exists(OutputPath))
            {
                File.Delete(OutputPath);
            }
        }

        private static void ResetRuntimeState()
        {
            readyAtFrame = 0;
            readyAtEditorTime = 0d;
            courtyardSelected = false;
            captureAttempted = false;
        }

        private static void ClearSessionState()
        {
            SessionState.EraseBool(ActiveKey);
            SessionState.EraseBool(BatchExitKey);
            SessionState.EraseInt(PhaseKey);
            SessionState.EraseString(FailureKey);
            SessionState.EraseString(StartedUtcTicksKey);
            SessionState.EraseString(BuildSettingsBeforeKey);
            SessionState.EraseString(SceneHashBeforeKey);
            SessionState.EraseString(PrefabHashBeforeKey);
            SessionState.EraseString(CatalogHashBeforeKey);
            ResetRuntimeState();
        }

        private readonly struct PixelAudit
        {
            public PixelAudit(float meanLuminance, float luminanceRange, bool isUsable)
            {
                MeanLuminance = meanLuminance;
                LuminanceRange = luminanceRange;
                IsUsable = isUsable;
            }

            public float MeanLuminance { get; }
            public float LuminanceRange { get; }
            public bool IsUsable { get; }
        }

        private sealed class CanvasCaptureState
        {
            public CanvasCaptureState(Canvas canvas)
            {
                Canvas = canvas;
                RenderMode = canvas.renderMode;
                WorldCamera = canvas.worldCamera;
                PlaneDistance = canvas.planeDistance;
                ScaleFactor = canvas.scaleFactor;
                Scaler = canvas.GetComponent<CanvasScaler>();
                ScalerEnabled = Scaler != null && Scaler.enabled;
            }

            public Canvas Canvas { get; }
            public RenderMode RenderMode { get; }
            public Camera WorldCamera { get; }
            public float PlaneDistance { get; }
            public float ScaleFactor { get; }
            public CanvasScaler Scaler { get; }
            public bool ScalerEnabled { get; }

            public void Restore()
            {
                if (Canvas == null)
                {
                    return;
                }

                Canvas.renderMode = RenderMode;
                Canvas.worldCamera = WorldCamera;
                Canvas.planeDistance = PlaneDistance;
                Canvas.scaleFactor = ScaleFactor;
                if (Scaler != null)
                {
                    Scaler.enabled = ScalerEnabled;
                }
            }
        }
    }
}
