using System.Collections;
using DimensionBrawl.LevelDesign;
using DimensionBrawl.UI;
using UnityEngine;
using UnityEngine.UI;

namespace DimensionBrawl.UI.StageClear
{
    [DisallowMultipleComponent]
    public sealed class StageClearScreenPresenter : MonoBehaviour
    {
        private static readonly string[] ProofRowObjectNames =
        {
            "Mission1",
            "Mission2",
            "Mission3"
        };

        private static readonly string[] ProofRowFrameObjectNames =
        {
            "Stage_Clear_UI_0001s_0010_Mission1_Frame",
            "Stage_Clear_UI_0001s_0007_Mission2_Frame",
            "Stage_Clear_UI_0001s_0004_Mission3_Frame"
        };

        private static readonly string[] ProofRowIconObjectNames =
        {
            "Stage_Clear_UI_0001s_0009_mission1_Icon",
            "Stage_Clear_UI_0001s_0006_mission2_Icon",
            "Stage_Clear_UI_0001s_0003_mission3_Icon"
        };

        [SerializeField] private Button retryButton;
        [SerializeField] private Button lobbyButton;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform motionRoot;
        [SerializeField] private MonoBehaviour uiRouteResolverBehaviour;
        [SerializeField] private StageResultPresentationCatalog presentationCatalog;
        [SerializeField] private string localeId = "ko-KR";
        [SerializeField] private Text primaryActionText;
        [SerializeField] private Text lobbyActionText;
        [SerializeField] private Text stageNameText;
        [SerializeField] private Text stageNumberText;
        [SerializeField] private Text totalActiveTimeLabelText;
        [SerializeField] private Text totalActiveTimeValueText;
        [SerializeField] private Text combatActiveTimeLabelText;
        [SerializeField] private Text combatActiveTimeValueText;
        [SerializeField] private Text recordsCategoryText;
        [SerializeField] private Text[] proofRowTexts = new Text[3];
        [Header("Clear Audio")]
        [SerializeField] private AudioSource clearBgmSource;
        [SerializeField] private AudioClip clearBgmClip;
        [SerializeField, Range(0f, 1f)] private float clearBgmVolume = 0.9f;
        [SerializeField] private bool playEntranceOnEnable = true;
        [SerializeField, Min(0f)] private float entranceDelaySeconds = 0.02f;
        [SerializeField, Min(0.01f)] private float entranceDurationSeconds = 0.42f;
        [SerializeField, Range(0.5f, 1f)] private float entranceStartScale = 0.94f;
        [SerializeField] private Vector2 entranceOffset = new Vector2(96f, -8f);

        private Coroutine entranceRoutine;
        private bool terminalHandoffPending;
        private RectTransform targetRect;
        private bool hasEntranceBaseTransform;
        private Vector2 entranceBasePosition;
        private Vector3 entranceBaseScale;
        private bool clearBgmPlayed;
        private bool entranceStarted;
        private bool entranceCompleted;
        private StageRunResultSummary resultSummary;
        private StageResultPresentationSnapshot presentationSnapshot;
        private StageResultPresentationAuditEnvelope presentationAudit;
        private StageRunActionSnapshot primaryAction;
        private StageRunActionSnapshot lobbyAction;

        public int RetryClickCount { get; private set; }
        public int PrimaryClickCount => RetryClickCount;
        public int LobbyClickCount { get; private set; }
        public bool IsConfigured => resultSummary != null;
        public StageRunResultSummary ResultSummary => resultSummary;
        public StageResultPresentationSnapshot PresentationSnapshot => presentationSnapshot;
        public StageResultPresentationAuditEnvelope PresentationAudit => presentationAudit;
        public string PrimaryActionId => primaryAction?.ActionId ?? string.Empty;
        public string LobbyActionId => lobbyAction?.ActionId ?? string.Empty;
        public string LastActionError { get; private set; } = string.Empty;
        public bool EntranceStartStateApplied { get; private set; }
        public bool EntranceStarted => entranceStarted;
        public bool IsEntrancePlaying => entranceRoutine != null;
        public bool EntranceCompleted => entranceCompleted;
        public int EntrancePlayCount { get; private set; }
        public int ClearBgmPlayCount { get; private set; }
        public bool IsTerminalHandoffPending => terminalHandoffPending;

        public void ConfigureResult(StageRunResultSummary summary)
        {
            ResolveButtons();
            ResolveMotionTargets();
            ResolveResultLabels();
            resultSummary = null;
            presentationSnapshot = null;
            presentationAudit = null;
            primaryAction = null;
            lobbyAction = null;
            LastActionError = string.Empty;
            ClearPresentationSurface();

            if (summary == null)
            {
                LastActionError = "Committed result summary is missing.";
                SetActionsInteractive(false);
                return;
            }

            for (int i = 0; i < summary.OfferedActionCount; i++)
            {
                StageRunActionSnapshot action = summary.GetOfferedAction(i);
                if (action.ActionKind == StageRouteActionKind.UIRoute
                    && action.TargetUiRouteId == StageUiRouteId.Lobby)
                {
                    lobbyAction = action;
                }
                else if ((summary.Outcome == StageRouteOutcome.Clear
                        && action.ActionKind == StageRouteActionKind.Replay)
                    || (summary.Outcome == StageRouteOutcome.Fail
                        && action.ActionKind == StageRouteActionKind.Retry))
                {
                    primaryAction = action;
                }
            }

            if (primaryAction == null || lobbyAction == null)
            {
                LastActionError = "Committed result does not offer the required outcome actions.";
                SetActionsInteractive(false);
                return;
            }

            StageResultPresentationSnapshot resolvedPresentation = null;
            StageResultPresentationAuditEnvelope resolvedAudit = null;
            string presentationProfileError = string.Empty;
            StageRunContext runContext = StageRunRuntime.ActiveContext;
            if (runContext == null
                || !StageRunRuntime.TryPrepareResultPresentation(
                    summary,
                    runContext.ResultProgressionJoinSnapshot,
                    localeId,
                    out resolvedPresentation,
                    out resolvedAudit,
                    out presentationProfileError))
            {
                LastActionError = string.IsNullOrWhiteSpace(presentationProfileError)
                    ? "Committed result presentation snapshot is unavailable."
                    : presentationProfileError;
                SetActionsInteractive(false);
                return;
            }

            IStageRunUiRouteResolver resolver = ResolveUiRouteResolver();
            StageRunUiRouteTarget lobbyTarget = null;
            string routeError = string.Empty;
            if (resolver == null
                || !resolver.TryResolve(
                    lobbyAction.TargetUiRouteId,
                    out lobbyTarget,
                    out routeError)
                || lobbyTarget == null
                || lobbyTarget.RouteId != lobbyAction.TargetUiRouteId)
            {
                LastActionError = string.IsNullOrWhiteSpace(routeError)
                    ? "Canonical Lobby route resolver is missing."
                    : routeError;
                SetActionsInteractive(false);
                return;
            }

            if (!StageRunRuntime.TryMarkResultPresented(
                    summary,
                    resolvedPresentation,
                    resolvedAudit,
                    out string presentationError))
            {
                LastActionError = presentationError;
                SetActionsInteractive(false);
                return;
            }

            resultSummary = summary;
            presentationSnapshot = resolvedPresentation;
            presentationAudit = resolvedAudit;
            ApplyPresentation(resolvedPresentation);
            SetActionsInteractive(false);
        }

        private void Awake()
        {
            ResolveButtons();
            ResolveMotionTargets();
            ApplyEntranceStartState();
        }

        private void OnEnable()
        {
            ResetEntranceCycle();
            ResolveButtons();
            ResolveMotionTargets();
            ResolveResultLabels();
            ApplyEntranceStartState();

            if (retryButton != null)
            {
                retryButton.onClick.AddListener(HandleRetryClicked);
            }

            if (lobbyButton != null)
            {
                lobbyButton.onClick.AddListener(HandleLobbyClicked);
            }

            SetActionsInteractive(false);
            if (!IsConfigured)
            {
                ClearPresentationSurface();
            }

            if (playEntranceOnEnable && IsConfigured)
            {
                PlayEntrance();
            }
        }

        private void OnDisable()
        {
            if (entranceRoutine != null)
            {
                StopCoroutine(entranceRoutine);
                entranceRoutine = null;
            }

            entranceStarted = false;
            entranceCompleted = false;
            EntranceStartStateApplied = false;

            if (retryButton != null)
            {
                retryButton.onClick.RemoveListener(HandleRetryClicked);
            }

            if (lobbyButton != null)
            {
                lobbyButton.onClick.RemoveListener(HandleLobbyClicked);
            }
        }

        public void PlayEntrance()
        {
            if (!isActiveAndEnabled || !IsConfigured || entranceStarted)
            {
                return;
            }

            ApplyEntranceStartState();
            entranceStarted = true;
            entranceCompleted = false;
            EntrancePlayCount++;
            PlayClearBgmOnce();
            entranceRoutine = StartCoroutine(EntranceRoutine());
        }

        private void HandleRetryClicked()
        {
            RetryClickCount++;
            DispatchAction(primaryAction);
        }

        private void HandleLobbyClicked()
        {
            LobbyClickCount++;
            DispatchAction(lobbyAction);
        }

        private void ResolveButtons()
        {
            retryButton ??= FindButton("RetryButton", "RetryButtonHitArea", "Retry", "RetryStageButton");
            lobbyButton ??= FindButton("LobbyButton", "LobbyButtonHitArea", "Lobby");
        }

        private void ResolveMotionTargets()
        {
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }

            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            targetRect ??= transform as RectTransform;
            if (motionRoot == null)
            {
                motionRoot = FindRectTransform("StageClearResponsiveRoot");
            }

            targetRect = motionRoot != null ? motionRoot : targetRect;
            if (targetRect != null && !hasEntranceBaseTransform)
            {
                entranceBasePosition = targetRect.anchoredPosition;
                entranceBaseScale = targetRect.localScale;
                hasEntranceBaseTransform = true;
            }
        }

        private IEnumerator EntranceRoutine()
        {
            ResolveMotionTargets();
            ResolveEntranceTransforms(
                out Vector2 startPosition,
                out Vector2 endPosition,
                out Vector3 startScale,
                out Vector3 endScale);

            if (entranceDelaySeconds > 0f)
            {
                yield return new WaitForSecondsRealtime(entranceDelaySeconds);
            }

            float duration = Mathf.Max(0.01f, entranceDurationSeconds);
            for (float elapsed = 0f; elapsed < duration; elapsed += Time.unscaledDeltaTime)
            {
                float t = Mathf.Clamp01(elapsed / duration);
                float eased = Mathf.Clamp01(1f - Mathf.Pow(1f - t, 3f));

                if (canvasGroup != null)
                {
                    canvasGroup.alpha = eased;
                }

                if (targetRect != null)
                {
                    targetRect.anchoredPosition = Vector2.LerpUnclamped(startPosition, endPosition, eased);
                    targetRect.localScale = Vector3.LerpUnclamped(startScale, endScale, eased);
                }

                yield return null;
            }

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
            }

            SetActionsInteractive(IsConfigured);

            if (targetRect != null)
            {
                targetRect.anchoredPosition = endPosition;
                targetRect.localScale = endScale;
            }

            entranceCompleted = true;
            entranceRoutine = null;
        }

        private void ResetEntranceCycle()
        {
            if (entranceRoutine != null)
            {
                StopCoroutine(entranceRoutine);
                entranceRoutine = null;
            }

            entranceStarted = false;
            entranceCompleted = false;
            EntranceStartStateApplied = false;
        }

        private void ApplyEntranceStartState()
        {
            ResolveMotionTargets();
            ResolveEntranceTransforms(
                out Vector2 startPosition,
                out _,
                out Vector3 startScale,
                out _);

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
            }

            SetActionsInteractive(false);
            if (targetRect != null)
            {
                targetRect.anchoredPosition = startPosition;
                targetRect.localScale = startScale;
            }

            EntranceStartStateApplied = true;
        }

        private void ResolveEntranceTransforms(
            out Vector2 startPosition,
            out Vector2 endPosition,
            out Vector3 startScale,
            out Vector3 endScale)
        {
            endScale = targetRect != null && hasEntranceBaseTransform
                ? entranceBaseScale
                : Vector3.one;
            endPosition = targetRect != null && hasEntranceBaseTransform
                ? entranceBasePosition
                : Vector2.zero;
            startPosition = endPosition + entranceOffset;
            startScale = new Vector3(
                endScale.x * entranceStartScale,
                endScale.y * entranceStartScale,
                endScale.z);
        }

        private Button FindButton(params string[] names)
        {
            Button[] buttons = GetComponentsInChildren<Button>(true);
            for (int nameIndex = 0; nameIndex < names.Length; nameIndex++)
            {
                string targetName = names[nameIndex];
                for (int buttonIndex = 0; buttonIndex < buttons.Length; buttonIndex++)
                {
                    Button candidate = buttons[buttonIndex];
                    if (candidate != null && candidate.name == targetName)
                    {
                        return candidate;
                    }
                }
            }

            return null;
        }

        private void PlayClearBgmOnce()
        {
            if (clearBgmPlayed
                || clearBgmClip == null
                || resultSummary == null
                || resultSummary.Outcome != StageRouteOutcome.Clear)
            {
                return;
            }

            clearBgmPlayed = true;
            AudioSource source = ResolveClearBgmSource();
            if (source == null)
            {
                return;
            }

            source.clip = null;
            source.loop = false;
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            source.priority = 24;
            source.volume = Mathf.Clamp01(clearBgmVolume);
            source.PlayOneShot(clearBgmClip, Mathf.Clamp01(clearBgmVolume));
            ClearBgmPlayCount++;
        }

        private AudioSource ResolveClearBgmSource()
        {
            if (clearBgmSource != null)
            {
                return clearBgmSource;
            }

            clearBgmSource = GetComponent<AudioSource>();
            if (clearBgmSource == null)
            {
                clearBgmSource = gameObject.AddComponent<AudioSource>();
            }

            return clearBgmSource;
        }

        private RectTransform FindRectTransform(string objectName)
        {
            RectTransform[] rectTransforms = GetComponentsInChildren<RectTransform>(true);
            for (int i = 0; i < rectTransforms.Length; i++)
            {
                RectTransform candidate = rectTransforms[i];
                if (candidate != null && candidate.name == objectName)
                {
                    return candidate;
                }
            }

            return null;
        }

        private void DispatchAction(StageRunActionSnapshot action)
        {
            if (resultSummary == null || action == null)
            {
                LastActionError = "Result action is unavailable.";
                SetActionsInteractive(false);
                return;
            }

            if (terminalHandoffPending)
            {
                return;
            }

            IStageRunUiRouteResolver resolver = ResolveUiRouteResolver();
            if (!UITransitionHandoffService.HasProvider)
            {
                DispatchTerminalActionImmediate(action, resolver);
                return;
            }

            if (!TryBuildTerminalHandoffDestination(
                    action,
                    resolver,
                    out UITransitionHandoffDestination destination,
                    out string routeError))
            {
                LastActionError = routeError;
                SetActionsInteractive(false);
                return;
            }

            terminalHandoffPending = true;
            SetActionsInteractive(false);
            if (!UITransitionHandoffService.TryBeginTerminalHandoff(
                    destination,
                    () => DispatchTerminalActionForHandoff(action, resolver),
                    HandleTerminalHandoffFailed,
                    out string beginError))
            {
                terminalHandoffPending = false;
                LastActionError = beginError;
                SetActionsInteractive(IsConfigured && EntranceCompleted);
                return;
            }
        }

        private UITransitionDispatchResult DispatchTerminalActionForHandoff(
            StageRunActionSnapshot action,
            IStageRunUiRouteResolver resolver)
        {
            if (!StageRunRuntime.TryDispatchTerminalAction(
                    resultSummary,
                    action.ActionId,
                    resolver,
                    out _,
                    out string error))
            {
                LastActionError = error;
                return UITransitionDispatchResult.Failure(error);
            }

            return UITransitionDispatchResult.Success();
        }

        private void HandleTerminalHandoffFailed(string error)
        {
            terminalHandoffPending = false;
            LastActionError = string.IsNullOrWhiteSpace(error)
                ? "The result transition handoff failed."
                : error;
            SetActionsInteractive(IsConfigured && EntranceCompleted);
        }

        private void DispatchTerminalActionImmediate(
            StageRunActionSnapshot action,
            IStageRunUiRouteResolver resolver)
        {
            if (!StageRunRuntime.TryDispatchTerminalAction(
                    resultSummary,
                    action.ActionId,
                    resolver,
                    out _,
                    out string error))
            {
                LastActionError = error;
                SetActionsInteractive(false);
            }
        }

        private bool TryBuildTerminalHandoffDestination(
            StageRunActionSnapshot action,
            IStageRunUiRouteResolver resolver,
            out UITransitionHandoffDestination destination,
            out string error)
        {
            destination = default;
            error = string.Empty;

            string sceneName;
            string scenePath;
            UITransitionDestinationKind destinationKind;
            if (action.ActionKind == StageRouteActionKind.Replay
                || action.ActionKind == StageRouteActionKind.Retry)
            {
                StageRunContext context = StageRunRuntime.ActiveContext;
                if (context == null
                    || !string.Equals(
                        action.TargetPlayableStageId,
                        context.Identity.PlayableStageId,
                        System.StringComparison.Ordinal))
                {
                    error = "The replay destination no longer matches the active stage run.";
                    return false;
                }

                StageRunSegmentSnapshot entry = context.RouteSnapshot.GetSegment(0);
                sceneName = entry.SceneName;
                scenePath = entry.ScenePath;
                destinationKind = UITransitionDestinationKind.Combat;
            }
            else if (action.ActionKind == StageRouteActionKind.UIRoute
                && resolver != null
                && resolver.TryResolve(
                    action.TargetUiRouteId,
                    out StageRunUiRouteTarget target,
                    out error)
                && target != null
                && target.RouteId == action.TargetUiRouteId)
            {
                sceneName = target.SceneName;
                scenePath = target.ScenePath;
                destinationKind = target.RouteId == StageUiRouteId.Lobby
                    ? UITransitionDestinationKind.Lobby
                    : UITransitionDestinationKind.None;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(error))
                {
                    error = "The result transition destination could not be resolved.";
                }

                return false;
            }

            if (destinationKind == UITransitionDestinationKind.None
                || string.IsNullOrWhiteSpace(sceneName)
                || string.IsNullOrWhiteSpace(scenePath))
            {
                error = "The result transition destination is incomplete.";
                return false;
            }

            destination = new UITransitionHandoffDestination(
                destinationKind,
                sceneName,
                scenePath);
            return destination.IsValid;
        }

        private IStageRunUiRouteResolver ResolveUiRouteResolver()
        {
            if (uiRouteResolverBehaviour is IStageRunUiRouteResolver configured)
            {
                return configured;
            }

            MonoBehaviour[] behaviours = GetComponents<MonoBehaviour>();
            for (int i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] is IStageRunUiRouteResolver candidate)
                {
                    uiRouteResolverBehaviour = behaviours[i];
                    return candidate;
                }
            }

            return null;
        }

        private void ResolveResultLabels()
        {
            primaryActionText ??= FindText("RetryText");
            lobbyActionText ??= FindText("NextStageText");
            stageNameText ??= FindText("StageName");
            stageNumberText ??= FindText("StageNumber");
            totalActiveTimeLabelText ??= FindText("MaxComboLabel");
            totalActiveTimeValueText ??= FindText("MaxComboValue");
            combatActiveTimeLabelText ??= FindText("BattleTimeLabel");
            combatActiveTimeValueText ??= FindText("BattleTimeValue");
            recordsCategoryText ??= FindText("MissionCategory");

            if (proofRowTexts == null || proofRowTexts.Length != ProofRowObjectNames.Length)
            {
                proofRowTexts = new Text[ProofRowObjectNames.Length];
            }

            for (int i = 0; i < proofRowTexts.Length; i++)
            {
                proofRowTexts[i] ??= FindText(ProofRowObjectNames[i]);
            }
        }

        private Text FindText(string objectName)
        {
            Text[] texts = GetComponentsInChildren<Text>(true);
            for (int i = 0; i < texts.Length; i++)
            {
                if (texts[i] != null && texts[i].name == objectName)
                {
                    return texts[i];
                }
            }

            return null;
        }

        private void ApplyPresentation(StageResultPresentationSnapshot snapshot)
        {
            bool clear = snapshot.Outcome == StageRouteOutcome.Clear;
            SetNamedObjectActive("StageName", true);
            SetNamedObjectActive("StageNumber", true);
            SetNamedObjectActive("MaxComboLabel", true);
            SetNamedObjectActive("MaxComboValue", true);
            SetNamedObjectActive("BattleTimeLabel", true);
            SetNamedObjectActive("BattleTimeValue", true);
            SetNamedObjectActive("Stage_Clear_UI_0000s_0011_MaxCombo_Frame", true);
            SetNamedObjectActive("Stage_Clear_UI_0000s_0010_MaxCombo_Icon", true);
            SetNamedObjectActive("Stage_Clear_UI_0000s_0007_BatteTime_Frame", true);
            SetNamedObjectActive("Stage_Clear_UI_0000s_0006_BatteTime_Icon", true);
            if (primaryActionText != null)
            {
                primaryActionText.text = snapshot.PrimaryActionLabel;
            }

            if (lobbyActionText != null)
            {
                lobbyActionText.text = snapshot.LobbyActionLabel;
            }

            if (stageNameText != null)
            {
                stageNameText.text = snapshot.StageTitle;
                stageNameText.color = snapshot.StageTitleColor;
            }

            if (stageNumberText != null)
            {
                stageNumberText.text = snapshot.StageCode;
            }

            if (totalActiveTimeLabelText != null)
            {
                totalActiveTimeLabelText.text = snapshot.TotalActiveTimeLabel;
            }

            if (totalActiveTimeValueText != null)
            {
                totalActiveTimeValueText.text = snapshot.TotalActiveTimeValue;
            }

            if (combatActiveTimeLabelText != null)
            {
                combatActiveTimeLabelText.text = snapshot.CombatActiveTimeLabel;
            }

            if (combatActiveTimeValueText != null)
            {
                combatActiveTimeValueText.text = snapshot.CombatActiveTimeValue;
            }

            if (recordsCategoryText != null)
            {
                recordsCategoryText.text = snapshot.RecordsCategoryLabel;
            }

            SetNamedObjectActive("Stage_Clear_UI_0000s_0000_StageClear_Icon", clear);
            SetNamedObjectActive("Stage_Clear_UI_0000s_0001_Claer!_Text", clear);
            SetNamedObjectActive("RewardCategory", false);
            SetNamedObjectActive("Stage_Clear_UI_0002s_0006_RewardCategory_Frame", false);
            SetNamedObjectActive("Stage_Clear_UI_0002s_0004_Reward_Panel", false);

            bool hasProofRows = snapshot.ProofRowCount > 0;
            SetNamedObjectActive("MissionCategory", hasProofRows);
            SetNamedObjectActive("Stage_Clear_UI_0001s_0000_MissionCategory_Frame", hasProofRows);
            for (int i = 0; i < ProofRowObjectNames.Length; i++)
            {
                bool active = i < snapshot.ProofRowCount;
                if (active && proofRowTexts[i] != null)
                {
                    proofRowTexts[i].text = snapshot.GetProofRow(i).LocalizedText;
                }

                SetNamedObjectActive(ProofRowObjectNames[i], active);
                SetNamedObjectActive(ProofRowFrameObjectNames[i], active);
                SetNamedObjectActive(ProofRowIconObjectNames[i], active);
            }
        }

        private void ClearPresentationSurface()
        {
            Text[] dynamicTexts =
            {
                primaryActionText,
                lobbyActionText,
                stageNameText,
                stageNumberText,
                totalActiveTimeLabelText,
                totalActiveTimeValueText,
                combatActiveTimeLabelText,
                combatActiveTimeValueText,
                recordsCategoryText
            };
            for (int i = 0; i < dynamicTexts.Length; i++)
            {
                if (dynamicTexts[i] != null)
                {
                    dynamicTexts[i].text = string.Empty;
                }
            }

            SetNamedObjectActive("StageName", false);
            SetNamedObjectActive("StageNumber", false);
            SetNamedObjectActive("MaxComboLabel", false);
            SetNamedObjectActive("MaxComboValue", false);
            SetNamedObjectActive("BattleTimeLabel", false);
            SetNamedObjectActive("BattleTimeValue", false);
            SetNamedObjectActive("Stage_Clear_UI_0000s_0011_MaxCombo_Frame", false);
            SetNamedObjectActive("Stage_Clear_UI_0000s_0010_MaxCombo_Icon", false);
            SetNamedObjectActive("Stage_Clear_UI_0000s_0007_BatteTime_Frame", false);
            SetNamedObjectActive("Stage_Clear_UI_0000s_0006_BatteTime_Icon", false);
            SetNamedObjectActive("Stage_Clear_UI_0000s_0000_StageClear_Icon", false);
            SetNamedObjectActive("Stage_Clear_UI_0000s_0001_Claer!_Text", false);
            SetNamedObjectActive("MissionCategory", false);
            SetNamedObjectActive("Stage_Clear_UI_0001s_0000_MissionCategory_Frame", false);
            SetNamedObjectActive("RewardCategory", false);
            SetNamedObjectActive("Stage_Clear_UI_0002s_0006_RewardCategory_Frame", false);
            SetNamedObjectActive("Stage_Clear_UI_0002s_0004_Reward_Panel", false);
            for (int i = 0; i < ProofRowObjectNames.Length; i++)
            {
                if (proofRowTexts != null && i < proofRowTexts.Length && proofRowTexts[i] != null)
                {
                    proofRowTexts[i].text = string.Empty;
                }

                SetNamedObjectActive(ProofRowObjectNames[i], false);
                SetNamedObjectActive(ProofRowFrameObjectNames[i], false);
                SetNamedObjectActive(ProofRowIconObjectNames[i], false);
            }
        }

        private void SetNamedObjectActive(string objectName, bool active)
        {
            RectTransform[] transforms = GetComponentsInChildren<RectTransform>(true);
            for (int i = 0; i < transforms.Length; i++)
            {
                if (transforms[i] != null && transforms[i].name == objectName)
                {
                    transforms[i].gameObject.SetActive(active);
                    return;
                }
            }
        }

        private void SetActionsInteractive(bool interactive)
        {
            if (retryButton != null)
            {
                retryButton.interactable = interactive && primaryAction != null;
            }

            if (lobbyButton != null)
            {
                lobbyButton.interactable = interactive && lobbyAction != null;
            }

            if (canvasGroup != null)
            {
                canvasGroup.interactable = interactive;
                canvasGroup.blocksRaycasts = interactive;
            }
        }
    }
}
