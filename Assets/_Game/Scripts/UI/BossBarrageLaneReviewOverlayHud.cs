using DimensionBrawl.LevelDesign;
using DimensionBrawl.Presentation;
using DimensionBrawl.Test;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

namespace DimensionBrawl.UI
{
    // Review-only overlay for pause, settings, and pocket results in the boss barrage lane scene.
    [DefaultExecutionOrder(1200)]
    [DisallowMultipleComponent]
    public sealed class BossBarrageLaneReviewOverlayHud : MonoBehaviour
    {
        private enum OverlayMode
        {
            None,
            Pause,
            Settings
        }

        [Header("References")]
        [SerializeField] private BossBarragePocketReviewOwner pocketReviewOwner;
        [SerializeField] private BossBarrageLaneReviewHud reviewHud;
        [SerializeField] private BossBarrageLaneReviewMobileHud mobileHud;
        [SerializeField] private ActionScreenCuePresenter screenCuePresenter;
        [SerializeField] private Behaviour[] inputLockBehaviours = new Behaviour[0];

        [Header("Routes")]
        [SerializeField] private string retrySceneName;
        [SerializeField] private string retryScenePath;
        [SerializeField] private string stageSelectSceneName;
        [SerializeField] private string stageSelectScenePath;
        [SerializeField] private string lobbySceneName;
        [SerializeField] private string lobbyScenePath;

        [Header("Display")]
        [SerializeField] private bool showOverlay = true;
        [SerializeField] private bool drawIdleButton = true;
        [SerializeField, Min(1f)] private float pauseButtonSize = 72f;
        [SerializeField, Min(0f)] private float edgeInset = 32f;
        [SerializeField, Min(1f)] private float panelWidth = 560f;
        [SerializeField, Min(1f)] private float panelHeight = 430f;
        [SerializeField] private Color panelColor = new Color(0.015f, 0.022f, 0.034f, 0.92f);
        [SerializeField] private Color dimColor = new Color(0f, 0f, 0f, 0.46f);
        [SerializeField] private Color accentColor = new Color(1f, 0.72f, 0.34f, 0.96f);
        [SerializeField] private Color clearAccentColor = new Color(0.28f, 1f, 0.62f, 0.96f);
        [SerializeField] private Color failAccentColor = new Color(1f, 0.28f, 0.2f, 0.96f);

        [Header("Input")]
        [SerializeField] private Key pauseKey = Key.Escape;

        private OverlayMode mode;
        private bool hasPausedTime;
        private bool hasCapturedControlState;
        private bool mobileHudEnabledBeforeOverlay;
        private bool[] inputLockEnabledBeforeOverlay = new bool[0];
        private float previousTimeScale = 1f;
        private float hudScale = 1f;
        private bool telemetryVisible;
        private bool screenCuesVisible = true;
        private GUIStyle titleStyle;
        private GUIStyle bodyStyle;
        private GUIStyle buttonStyle;
        private GUIStyle primaryButtonStyle;
        private GUIStyle smallButtonStyle;
        private Texture2D solidTexture;
        private float cachedStyleScale = -1f;
        private BossBarragePocketReviewOwner subscribedPocketReviewOwner;
        private InputAction pauseAction;
        private bool resultControlsLocked;

        public BossBarragePocketReviewOwner PocketReviewOwner => pocketReviewOwner;
        public BossBarrageLaneReviewHud ReviewHud => reviewHud;
        public BossBarrageLaneReviewMobileHud MobileHud => mobileHud;
        public ActionScreenCuePresenter ScreenCuePresenter => screenCuePresenter;
        public string RetrySceneName => retrySceneName;
        public string RetryScenePath => retryScenePath;
        public string StageSelectSceneName => stageSelectSceneName;
        public string StageSelectScenePath => stageSelectScenePath;
        public string LobbySceneName => lobbySceneName;
        public string LobbyScenePath => lobbyScenePath;
        public bool IsPauseMenuVisible => mode == OverlayMode.Pause;
        public bool IsSettingsVisible => mode == OverlayMode.Settings;
        public bool IsResultVisible => HasResult;
        public string ResultTitleReadout => HasResult ? ResolveResultTitle() : string.Empty;
        public string ResultSummaryReadout => HasResult ? ResolveResultSummary() : string.Empty;
        public string ResultRewardReadout => HasResult ? ResolveResultRewardHook() : string.Empty;
        public string ResultNextObjectiveReadout => HasResult ? ResolveResultNextObjective() : string.Empty;
        public string ResultRouteReadout => HasResult ? ResolveResultRouteLabel() : string.Empty;

        public void Configure(
            BossBarragePocketReviewOwner newPocketReviewOwner,
            BossBarrageLaneReviewHud newReviewHud,
            BossBarrageLaneReviewMobileHud newMobileHud,
            ActionScreenCuePresenter newScreenCuePresenter)
        {
            UnsubscribePocketResult();
            pocketReviewOwner = newPocketReviewOwner;
            reviewHud = newReviewHud;
            mobileHud = newMobileHud;
            screenCuePresenter = newScreenCuePresenter;
            CaptureSettings();
            if (isActiveAndEnabled)
            {
                SubscribePocketResult();
                RefreshOverlayState();
            }
        }

        public void ConfigureInputLocks(params Behaviour[] newInputLockBehaviours)
        {
            inputLockBehaviours = newInputLockBehaviours != null
                ? newInputLockBehaviours
                : new Behaviour[0];
            inputLockEnabledBeforeOverlay = new bool[inputLockBehaviours.Length];
            hasCapturedControlState = false;
        }

        public void ConfigureRoutes(
            string newRetrySceneName,
            string newRetryScenePath,
            string newStageSelectSceneName,
            string newStageSelectScenePath,
            string newLobbySceneName,
            string newLobbyScenePath)
        {
            retrySceneName = newRetrySceneName;
            retryScenePath = newRetryScenePath;
            stageSelectSceneName = newStageSelectSceneName;
            stageSelectScenePath = newStageSelectScenePath;
            lobbySceneName = newLobbySceneName;
            lobbyScenePath = newLobbyScenePath;
        }

        public void OpenPauseMenu()
        {
            if (HasResult)
            {
                return;
            }

            if (!hasPausedTime)
            {
                previousTimeScale = Time.timeScale;
                Time.timeScale = 0f;
                hasPausedTime = true;
            }

            mode = OverlayMode.Pause;
            DisableGameplayControls();
        }

        public void OpenSettings()
        {
            if (!HasResult)
            {
                OpenPauseMenu();
                mode = OverlayMode.Settings;
            }
        }

        public void Resume()
        {
            if (HasResult)
            {
                return;
            }

            RestoreTimeScale();
            mode = OverlayMode.None;
            RestoreGameplayControls();
        }

        private bool HasResult => pocketReviewOwner != null
            && (pocketReviewOwner.IsCleared || pocketReviewOwner.IsFailed);

        private void Awake()
        {
            if (reviewHud == null)
            {
                reviewHud = GetComponent<BossBarrageLaneReviewHud>();
            }

            if (mobileHud == null)
            {
                mobileHud = GetComponent<BossBarrageLaneReviewMobileHud>();
            }

            if (screenCuePresenter == null)
            {
                screenCuePresenter = GetComponent<ActionScreenCuePresenter>();
            }

            CaptureSettings();
        }

        private void OnEnable()
        {
            SubscribePocketResult();
            EnablePauseInput();
            RefreshOverlayState();
        }

        private void OnDisable()
        {
            DisablePauseInput();
            UnsubscribePocketResult();
            resultControlsLocked = false;
            RestoreTimeScale();
            RestoreGameplayControls();
        }

        private void RefreshOverlayState()
        {
            if (!showOverlay)
            {
                resultControlsLocked = false;
                RestoreTimeScale();
                RestoreGameplayControls();
                return;
            }

            if (HasResult)
            {
                RestoreTimeScale();
                mode = OverlayMode.None;
                if (!resultControlsLocked)
                {
                    DisableGameplayControls();
                    resultControlsLocked = true;
                }

                return;
            }

            if (resultControlsLocked)
            {
                resultControlsLocked = false;
                RestoreGameplayControls();
            }
        }

        private void OnGUI()
        {
            bool hasResult = HasResult;
            if (!showOverlay || (!hasResult && mode == OverlayMode.None && !drawIdleButton))
            {
                return;
            }

            float scale = ResolveScale();
            EnsureStyles(scale);
            int previousDepth = GUI.depth;
            GUI.depth = -2200;

            if (hasResult)
            {
                DrawResultOverlay();
            }
            else if (mode == OverlayMode.Pause)
            {
                DrawPauseOverlay();
            }
            else if (mode == OverlayMode.Settings)
            {
                DrawSettingsOverlay();
            }
            else
            {
                if (drawIdleButton)
                {
                    DrawPauseButton();
                }
            }

            GUI.depth = previousDepth;
        }

        private void SubscribePocketResult()
        {
            if (subscribedPocketReviewOwner == pocketReviewOwner)
            {
                return;
            }

            UnsubscribePocketResult();
            subscribedPocketReviewOwner = pocketReviewOwner;
            if (subscribedPocketReviewOwner == null)
            {
                return;
            }

            subscribedPocketReviewOwner.PocketCleared += HandlePocketResult;
            subscribedPocketReviewOwner.PocketFailed += HandlePocketResult;
        }

        private void UnsubscribePocketResult()
        {
            if (subscribedPocketReviewOwner == null)
            {
                return;
            }

            subscribedPocketReviewOwner.PocketCleared -= HandlePocketResult;
            subscribedPocketReviewOwner.PocketFailed -= HandlePocketResult;
            subscribedPocketReviewOwner = null;
        }

        private void HandlePocketResult()
        {
            RefreshOverlayState();
        }

        private void EnablePauseInput()
        {
            if (pauseAction != null || pauseKey == Key.None)
            {
                return;
            }

            pauseAction = new InputAction(
                "BossBarrageReviewPause",
                InputActionType.Button,
                $"<Keyboard>/{pauseKey}");
            pauseAction.performed += HandlePausePerformed;
            pauseAction.Enable();
        }

        private void DisablePauseInput()
        {
            if (pauseAction == null)
            {
                return;
            }

            pauseAction.performed -= HandlePausePerformed;
            pauseAction.Disable();
            pauseAction.Dispose();
            pauseAction = null;
        }

        private void HandlePausePerformed(InputAction.CallbackContext context)
        {
            if (!showOverlay || HasResult)
            {
                return;
            }

            if (mode == OverlayMode.None)
            {
                OpenPauseMenu();
            }
            else
            {
                Resume();
            }
        }

        private void DrawPauseButton()
        {
            float scale = ResolveScale();
            float size = pauseButtonSize * scale;
            float inset = edgeInset * scale;
            Rect rect = new Rect(Screen.width - inset - size, inset, size, size);
            if (DrawImmediateButton(rect, "II", smallButtonStyle))
            {
                OpenPauseMenu();
            }
        }

        private void DrawPauseOverlay()
        {
            Rect panel = BeginModal("PAUSED", "Combat flow is held.");
            if (DrawMenuButton("CONTINUE", primary: true))
            {
                Resume();
            }

            if (DrawMenuButton("RETRY", primary: false))
            {
                LoadRetryScene();
            }

            if (DrawMenuButton("SETTINGS", primary: false))
            {
                mode = OverlayMode.Settings;
            }

            if (DrawMenuButton("STAGE SELECT", primary: false))
            {
                LoadConfiguredScene(stageSelectSceneName, stageSelectScenePath);
            }

            if (DrawMenuButton("LOBBY", primary: false))
            {
                LoadConfiguredScene(lobbySceneName, lobbyScenePath);
            }

            EndModal(panel);
        }

        private void DrawSettingsOverlay()
        {
            float scale = ResolveScale();
            Rect panel = BeginModal("SETTINGS", "Review tuning");
            GUILayout.Label($"HUD Scale {hudScale:0.00}", bodyStyle);
            float newScale = GUILayout.HorizontalSlider(hudScale, 0.8f, 1.35f);
            if (!Mathf.Approximately(newScale, hudScale))
            {
                hudScale = newScale;
                mobileHud?.SetHudScale(hudScale);
            }

            GUILayout.Space(12f * scale);
            bool newTelemetry = GUILayout.Toggle(telemetryVisible, "Detailed Telemetry", bodyStyle);
            if (newTelemetry != telemetryVisible)
            {
                telemetryVisible = newTelemetry;
                reviewHud?.SetDetailedTelemetryVisible(telemetryVisible);
            }

            bool newScreenCues = GUILayout.Toggle(screenCuesVisible, "Screen Cues", bodyStyle);
            if (newScreenCues != screenCuesVisible)
            {
                screenCuesVisible = newScreenCues;
                screenCuePresenter?.SetScreenCuesVisible(screenCuesVisible);
            }

            GUILayout.FlexibleSpace();
            if (DrawMenuButton("BACK", primary: false))
            {
                mode = OverlayMode.Pause;
            }

            if (DrawMenuButton("CONTINUE", primary: true))
            {
                Resume();
            }

            EndModal(panel);
        }

        private void DrawResultOverlay()
        {
            bool cleared = pocketReviewOwner != null && pocketReviewOwner.IsCleared;
            string title = ResolveResultTitle();
            string summary = ResolveResultSummary();
            Color resultAccent = cleared ? clearAccentColor : failAccentColor;
            bool hasCommittedRecord = TryGetCommittedResultRecord(
                out BossBarragePocketReviewOwner.RouteResultRecord resultRecord);

            Rect panel = BeginModal(title, summary, resultAccent);
            float resultSeconds = hasCommittedRecord ? resultRecord.ElapsedSeconds : pocketReviewOwner.ResultElapsedSeconds;
            int completedObjectives = hasCommittedRecord
                ? resultRecord.CompletedObjectiveStepCount
                : pocketReviewOwner.CompletedObjectiveStepCount;
            int objectiveSteps = hasCommittedRecord ? resultRecord.ObjectiveStepCount : pocketReviewOwner.ObjectiveStepCount;
            GUILayout.Label(ResolveResultLine("Time", $"{resultSeconds:0.0}s"), bodyStyle);
            GUILayout.Label(
                ResolveResultLine(
                    "Objectives",
                    $"{completedObjectives}/{objectiveSteps}"),
                bodyStyle);
            GUILayout.Label(
                ResolveResultLine("Survival", ResolveResultRouteLabel()),
                bodyStyle);
            GUILayout.Label(
                ResolveResultLine("Next", ResolveResultNextObjective()),
                bodyStyle);
            GUILayout.Space(18f);

            if (DrawMenuButton("RETRY", primary: true))
            {
                LoadRetryScene();
            }

            if (DrawMenuButton("STAGE SELECT", primary: false))
            {
                LoadConfiguredScene(stageSelectSceneName, stageSelectScenePath);
            }

            if (DrawMenuButton("LOBBY", primary: false))
            {
                LoadConfiguredScene(lobbySceneName, lobbyScenePath);
            }

            EndModal(panel);
        }

        private Rect BeginModal(string title, string subtitle)
        {
            return BeginModal(title, subtitle, accentColor);
        }

        private Rect BeginModal(string title, string subtitle, Color accent)
        {
            DrawSolid(new Rect(0f, 0f, Screen.width, Screen.height), dimColor);
            float scale = ResolveScale();
            float inset = edgeInset * scale;
            float wideBlend = Mathf.Clamp01((Screen.width - 1280f) / 1920f);
            float responsiveWidth = Mathf.Max(
                panelWidth * scale,
                Mathf.Min(Screen.width * Mathf.Lerp(0.34f, 0.42f, wideBlend), 980f * scale));
            float responsiveHeight = Mathf.Max(
                panelHeight * scale,
                Mathf.Min(Screen.height * 0.46f, 680f * scale));
            float width = Mathf.Min(responsiveWidth, Screen.width - inset * 2f);
            float height = Mathf.Min(responsiveHeight, Screen.height - inset * 2f);
            Rect panel = new Rect((Screen.width - width) * 0.5f, (Screen.height - height) * 0.5f, width, height);
            DrawSolid(panel, panelColor);
            DrawBorder(panel, accent, Mathf.Max(2f, 2f * scale));

            float horizontalPadding = 28f * scale;
            float verticalPadding = 24f * scale;
            GUILayout.BeginArea(
                new Rect(
                    panel.x + horizontalPadding,
                    panel.y + verticalPadding,
                    panel.width - horizontalPadding * 2f,
                    panel.height - verticalPadding * 2f));
            titleStyle.normal.textColor = accent;
            GUILayout.Label(title, titleStyle);
            GUILayout.Label(subtitle, bodyStyle);
            GUILayout.Space(18f * scale);
            return panel;
        }

        private static void EndModal(Rect panel)
        {
            _ = panel;
            GUILayout.EndArea();
        }

        private bool DrawMenuButton(string label, bool primary)
        {
            float scale = ResolveScale();
            GUILayout.Space(6f * scale);
            GUIStyle style = primary ? primaryButtonStyle : buttonStyle;
            Rect rect = GUILayoutUtility.GetRect(
                new GUIContent(label),
                style,
                GUILayout.Height(Mathf.Clamp(48f * scale, 42f, 82f)));
            return DrawImmediateButton(rect, label, style);
        }

        private static bool DrawImmediateButton(Rect rect, string label, GUIStyle style)
        {
            GUI.Label(rect, label, style);
            Event current = Event.current;
            if (current == null
                || current.type != EventType.MouseDown
                || current.button != 0
                || !rect.Contains(current.mousePosition))
            {
                return false;
            }

            current.Use();
            return true;
        }

        private string ResolveResultTitle()
        {
            if (TryGetCommittedResultRecord(out BossBarragePocketReviewOwner.RouteResultRecord resultRecord)
                && !string.IsNullOrWhiteSpace(resultRecord.Title))
            {
                return resultRecord.Title;
            }

            FrontlineWaveStageProfile profile = ResolveActiveStageProfile();
            if (pocketReviewOwner == null)
            {
                return string.Empty;
            }

            if (pocketReviewOwner.IsCleared)
            {
                return ResolveStageText(profile?.ClearTitle, "PRESSURE BROKEN");
            }

            if (pocketReviewOwner.IsFailed)
            {
                return ResolveStageText(profile?.FailTitle, "PLAYER DOWN");
            }

            return string.Empty;
        }

        private string ResolveResultSummary()
        {
            if (TryGetCommittedResultRecord(out BossBarragePocketReviewOwner.RouteResultRecord resultRecord)
                && !string.IsNullOrWhiteSpace(resultRecord.Summary))
            {
                return resultRecord.Summary;
            }

            FrontlineWaveStageProfile profile = ResolveActiveStageProfile();
            if (pocketReviewOwner == null)
            {
                return string.Empty;
            }

            if (pocketReviewOwner.IsCleared)
            {
                if (IsCounterRecoveryClear())
                {
                    return ResolveStageText(
                        profile?.ClearCounterDetail,
                        "Counter pressure held; final follow-up confirmed");
                }

                if (pocketReviewOwner.Skill1FollowupHitConfirmed)
                {
                    return ResolveStageText(
                        profile?.ClearFollowupDetail,
                        "Summon opening confirmed; Skill1 follow-up landed");
                }

                return ResolveStageText(
                    profile?.ClearPressureDetail,
                    "Boss curtain suppressed; survival answer recorded");
            }

            if (pocketReviewOwner.IsFailed)
            {
                if (pocketReviewOwner.FailedFromRouteStabilityCollapse)
                {
                    return ResolveStageText(
                        profile?.RouteCollapseFailDetail,
                        "Pressure control hit zero, but HP survival remains the fail state");
                }

                return ResolveStageText(
                    profile?.FailDetail,
                    "Player HP reached zero before the boss pressure was answered");
            }

            return string.Empty;
        }

        private string ResolveResultRewardHook()
        {
            if (TryGetCommittedResultRecord(out BossBarragePocketReviewOwner.RouteResultRecord resultRecord)
                && !string.IsNullOrWhiteSpace(resultRecord.RewardHook))
            {
                return resultRecord.RewardHook;
            }

            FrontlineWaveStageProfile profile = ResolveActiveStageProfile();
            if (pocketReviewOwner == null)
            {
                return string.Empty;
            }

            if (pocketReviewOwner.IsFailed)
            {
                return ResolveStageText(
                    profile?.FailedRouteRewardHook,
                    "Failure analysis logged: player HP reached zero before the answer was complete.");
            }

            if (IsCounterRecoveryClear())
            {
                return ResolveStageText(
                    profile?.CounterRecoveryRewardHook,
                    "Counter recovery logged: summon absorbed pressure and reopened the final strike window.");
            }

            if (pocketReviewOwner.IsCleared && pocketReviewOwner.Skill1FollowupHitConfirmed)
            {
                return ResolveStageText(
                    profile?.CleanRouteRewardHook,
                    "Clean survival logged: summon cover created a Skill1 confirm before counter pressure arrived.");
            }

            return ResolveStageText(
                profile?.RewardHook,
                "No payout or progression grant.");
        }

        private string ResolveResultNextObjective()
        {
            if (TryGetCommittedResultRecord(out BossBarragePocketReviewOwner.RouteResultRecord resultRecord)
                && !string.IsNullOrWhiteSpace(resultRecord.NextObjective))
            {
                return resultRecord.NextObjective;
            }

            FrontlineWaveStageProfile profile = ResolveActiveStageProfile();
            if (pocketReviewOwner == null)
            {
                return string.Empty;
            }

            if (pocketReviewOwner.IsFailed)
            {
                return ResolveStageText(
                    profile?.FailedRouteNextObjective,
                    "Next run: protect HP first, then spend summon on the visible curtain.");
            }

            if (IsCounterRecoveryClear())
            {
                return ResolveStageText(
                    profile?.CounterRecoveryNextObjective,
                    "Next run: answer counter pressure earlier so recovery becomes a clean survival answer.");
            }

            if (pocketReviewOwner.IsCleared)
            {
                return ResolveStageText(
                    profile?.CleanRouteNextObjective,
                    "Next run: keep HP clean by confirming before counter pressure enters.");
            }

            return string.Empty;
        }

        private string ResolveResultRouteLabel()
        {
            if (TryGetCommittedResultRecord(out BossBarragePocketReviewOwner.RouteResultRecord resultRecord)
                && !string.IsNullOrWhiteSpace(resultRecord.RouteLabel))
            {
                return resultRecord.RouteLabel;
            }

            if (pocketReviewOwner == null)
            {
                return "-";
            }

            if (pocketReviewOwner.IsFailed)
            {
                return pocketReviewOwner.FailedFromRouteStabilityCollapse
                    ? "Pressure control zero"
                    : "Player down";
            }

            if (IsCounterRecoveryClear())
            {
                return "Counter recovery";
            }

            return pocketReviewOwner.Skill1FollowupHitConfirmed
                ? "Clean summon follow-up"
                : "Pressure suppression";
        }

        private bool TryGetCommittedResultRecord(out BossBarragePocketReviewOwner.RouteResultRecord resultRecord)
        {
            if (pocketReviewOwner != null && pocketReviewOwner.HasCommittedResultRecord)
            {
                resultRecord = pocketReviewOwner.LastResultRecord;
                return true;
            }

            resultRecord = default;
            return false;
        }

        private FrontlineWaveStageProfile ResolveActiveStageProfile()
        {
            if (pocketReviewOwner != null && pocketReviewOwner.StageProfile != null)
            {
                return pocketReviewOwner.StageProfile;
            }

            return reviewHud != null ? reviewHud.StageProfileForReview : null;
        }

        private bool IsCounterRecoveryClear()
        {
            return pocketReviewOwner != null
                && pocketReviewOwner.Skill1FollowupHitConfirmed
                && (pocketReviewOwner.IsCounterWaveStabilized || pocketReviewOwner.IsCounterWaveFinalWindowOpened);
        }

        private static string ResolveStageText(string profileText, string fallback)
        {
            return string.IsNullOrWhiteSpace(profileText) ? fallback : profileText;
        }

        private static string ResolveResultLine(string label, string value)
        {
            return $"{label}: {value}";
        }

        private void CaptureSettings()
        {
            hudScale = mobileHud != null ? mobileHud.HudScale : 1f;
            telemetryVisible = reviewHud != null && reviewHud.ShowDetailedTelemetry;
            screenCuesVisible = screenCuePresenter == null || screenCuePresenter.ShowScreenCues;
        }

        private void LoadConfiguredScene(string sceneName, string scenePath)
        {
            RestoreTimeScale();
            Time.timeScale = 1f;
            DisableGameplayControls();

#if UNITY_EDITOR
            if (!string.IsNullOrWhiteSpace(scenePath))
            {
                EditorSceneManager.LoadSceneInPlayMode(scenePath, new LoadSceneParameters(LoadSceneMode.Single));
                return;
            }
#endif

            if (!string.IsNullOrWhiteSpace(sceneName))
            {
                SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
                return;
            }

            Debug.LogWarning("Review overlay scene route is not configured.", this);
        }

        private void LoadRetryScene()
        {
            RestoreTimeScale();
            Time.timeScale = 1f;
            DisableGameplayControls();

            Scene activeScene = SceneManager.GetActiveScene();
#if UNITY_EDITOR
            if (activeScene.IsValid() && !string.IsNullOrWhiteSpace(activeScene.path))
            {
                EditorSceneManager.LoadSceneInPlayMode(
                    activeScene.path,
                    new LoadSceneParameters(LoadSceneMode.Single));
                return;
            }
#endif

            if (activeScene.IsValid() && !string.IsNullOrWhiteSpace(activeScene.name))
            {
                SceneManager.LoadScene(activeScene.name, LoadSceneMode.Single);
                return;
            }

            LoadConfiguredScene(retrySceneName, retryScenePath);
        }

        private void RestoreTimeScale()
        {
            if (!hasPausedTime)
            {
                return;
            }

            Time.timeScale = previousTimeScale;
            hasPausedTime = false;
        }

        private void SetMobileControlsEnabled(bool enabled)
        {
            if (mobileHud != null && mobileHud.enabled != enabled)
            {
                mobileHud.enabled = enabled;
            }
        }

        private void DisableGameplayControls()
        {
            CaptureControlState();
            SetMobileControlsEnabled(false);
            SetInputLockBehavioursEnabled(false);
        }

        private void RestoreGameplayControls()
        {
            if (!hasCapturedControlState)
            {
                return;
            }

            SetMobileControlsEnabled(mobileHudEnabledBeforeOverlay);

            int lockCount = inputLockBehaviours != null ? inputLockBehaviours.Length : 0;
            for (int i = 0; i < lockCount; i++)
            {
                bool wasEnabled = i < inputLockEnabledBeforeOverlay.Length && inputLockEnabledBeforeOverlay[i];
                SetBehaviourEnabled(inputLockBehaviours[i], wasEnabled);
            }

            hasCapturedControlState = false;
        }

        private void CaptureControlState()
        {
            if (hasCapturedControlState)
            {
                return;
            }

            mobileHudEnabledBeforeOverlay = mobileHud != null && mobileHud.enabled;
            int lockCount = inputLockBehaviours != null ? inputLockBehaviours.Length : 0;
            if (inputLockEnabledBeforeOverlay == null || inputLockEnabledBeforeOverlay.Length != lockCount)
            {
                inputLockEnabledBeforeOverlay = new bool[lockCount];
            }

            for (int i = 0; i < lockCount; i++)
            {
                Behaviour behaviour = inputLockBehaviours[i];
                inputLockEnabledBeforeOverlay[i] = behaviour != null && behaviour.enabled;
            }

            hasCapturedControlState = true;
        }

        private void SetInputLockBehavioursEnabled(bool enabled)
        {
            int lockCount = inputLockBehaviours != null ? inputLockBehaviours.Length : 0;
            for (int i = 0; i < lockCount; i++)
            {
                SetBehaviourEnabled(inputLockBehaviours[i], enabled);
            }
        }

        private static void SetBehaviourEnabled(Behaviour behaviour, bool enabled)
        {
            if (behaviour != null && behaviour.enabled != enabled)
            {
                behaviour.enabled = enabled;
            }
        }

        private float ResolveScale()
        {
            float widthScale = Screen.width / 2560f;
            float heightScale = Screen.height / 1440f;
            return Mathf.Clamp(Mathf.Lerp(heightScale, widthScale, 0.35f), 0.82f, 1.6f);
        }

        private void EnsureStyles(float scale)
        {
            if (titleStyle != null && Mathf.Abs(cachedStyleScale - scale) < 0.025f)
            {
                return;
            }

            cachedStyleScale = scale;
            int titleFontSize = Mathf.RoundToInt(Mathf.Clamp(30f * scale, 24f, 48f));
            int bodyFontSize = Mathf.RoundToInt(Mathf.Clamp(17f * scale, 15f, 28f));
            int buttonFontSize = Mathf.RoundToInt(Mathf.Clamp(18f * scale, 16f, 30f));
            titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = titleFontSize,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = accentColor }
            };
            bodyStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = bodyFontSize,
                wordWrap = true,
                normal = { textColor = new Color(0.9f, 0.96f, 1f, 0.94f) }
            };
            buttonStyle = CreateButtonStyle(new Color(0.05f, 0.08f, 0.12f, 0.9f), Color.white, buttonFontSize);
            primaryButtonStyle = CreateButtonStyle(new Color(0.9f, 0.56f, 0.18f, 0.96f), Color.black, buttonFontSize);
            smallButtonStyle = CreateButtonStyle(
                new Color(0.015f, 0.022f, 0.034f, 0.82f),
                Color.white,
                buttonFontSize);
        }

        private static GUIStyle CreateButtonStyle(Color background, Color text, int fontSize)
        {
            var style = new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                fontSize = fontSize,
                normal = { textColor = text },
                hover = { textColor = text },
                active = { textColor = text },
                wordWrap = true
            };
            style.normal.background = MakeTexture(background);
            style.hover.background = MakeTexture(new Color(background.r + 0.05f, background.g + 0.05f, background.b + 0.05f, background.a));
            style.active.background = MakeTexture(new Color(background.r + 0.1f, background.g + 0.1f, background.b + 0.1f, background.a));
            return style;
        }

        private void DrawSolid(Rect rect, Color color)
        {
            if (solidTexture == null)
            {
                solidTexture = MakeTexture(Color.white);
            }

            Color previous = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, solidTexture);
            GUI.color = previous;
        }

        private void DrawBorder(Rect rect, Color color, float thickness)
        {
            DrawSolid(new Rect(rect.x, rect.y, rect.width, thickness), color);
            DrawSolid(new Rect(rect.x, rect.yMax - thickness, rect.width, thickness), color);
            DrawSolid(new Rect(rect.x, rect.y, thickness, rect.height), color);
            DrawSolid(new Rect(rect.xMax - thickness, rect.y, thickness, rect.height), color);
        }

        private static Texture2D MakeTexture(Color color)
        {
            var texture = new Texture2D(1, 1)
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }
    }
}
