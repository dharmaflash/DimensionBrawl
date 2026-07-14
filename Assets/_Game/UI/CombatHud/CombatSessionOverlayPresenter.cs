using System;
using System.Collections;
using DimensionBrawl.Combat;
using DimensionBrawl.Presentation;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace DimensionBrawl.UI
{
    [DisallowMultipleComponent]
    public sealed class CombatSessionOverlayPresenter : MonoBehaviour, ICombatSessionOverlay
    {
        [Header("Routing")]
        [SerializeField] private UIScreenRouteTable routeTable;
        [SerializeField] private UISceneRouteLoader routeLoader;
        [SerializeField] private UIRouteId retryRoute = UIRouteId.Combat;
        [SerializeField] private UIRouteId stageSelectRoute = UIRouteId.StageSelect;
        [SerializeField] private UIRouteId lobbyRoute = UIRouteId.Lobby;

        [Header("Surface")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Image accentImage;
        [SerializeField] private Image modeIcon;
        [SerializeField] private Sprite pauseIcon;
        [SerializeField] private Sprite settingsIcon;
        [SerializeField] private Text titleText;
        [SerializeField] private Text bodyText;
        [SerializeField] private Text detailText;
        [SerializeField] private Text routeStatusText;
        [SerializeField] private GameObject settingsContent;
        [SerializeField] private Toggle screenCuesToggle;

        [Header("Actions")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button stageSelectButton;
        [SerializeField] private Button lobbyButton;
        [SerializeField] private Button backButton;

        [Header("Presentation")]
        [SerializeField] private Color pauseAccent = new(0.14f, 0.86f, 0.94f, 1f);
        [SerializeField] private Color settingsAccent = new(0.98f, 0.78f, 0.28f, 1f);
        [SerializeField] private Color failureAccent = new(1f, 0.28f, 0.24f, 1f);

        [Header("Input")]
        [SerializeField] private Key pauseKey = Key.Escape;

        private BossBarrageEncounterController resultSource;
        private ActionScreenCuePresenter screenCuePresenter;
        private CombatSessionOverlayMode mode;
        private CombatSessionOverlayMode modeBeforeRouting;
        private InputAction pauseAction;
        private Coroutine routingRoutine;
        private LayoutGroup[] managedLayoutGroups = Array.Empty<LayoutGroup>();
        private bool[] authoredLayoutGroupStates = Array.Empty<bool>();
        private bool hasPausedTime;
        private float previousTimeScale = 1f;
        private bool publishedInputBlock;

        public CombatSessionOverlayMode Mode => mode;
        public bool IsVisible => mode != CombatSessionOverlayMode.Hidden;

        public event Action<bool> CombatInputBlockChanged;

        public void Configure(
            BossBarrageEncounterController newResultSource,
            ActionScreenCuePresenter newScreenCuePresenter)
        {
            resultSource = newResultSource;
            screenCuePresenter = newScreenCuePresenter;
            SyncScreenCueToggle();

            if (resultSource != null && resultSource.IsFailed)
            {
                ShowFailure();
            }
        }

        private void Awake()
        {
            managedLayoutGroups = GetComponentsInChildren<LayoutGroup>(includeInactive: true);
            authoredLayoutGroupStates = new bool[managedLayoutGroups.Length];
            for (int i = 0; i < managedLayoutGroups.Length; i++)
            {
                authoredLayoutGroupStates[i] = managedLayoutGroups[i] != null
                    && managedLayoutGroups[i].enabled;
            }

            SetCanvasVisible(false);
        }

        private void OnEnable()
        {
            AddButtonListeners();
            EnablePauseInput();
            SyncScreenCueToggle();
        }

        private void OnDisable()
        {
            RemoveButtonListeners();
            DisablePauseInput();

            if (routingRoutine != null)
            {
                StopCoroutine(routingRoutine);
                routingRoutine = null;
            }

            RestoreTimeScale();
            mode = CombatSessionOverlayMode.Hidden;
            SetCanvasVisible(false);
            PublishCombatInputBlock(false);
        }

        public void ShowPause()
        {
            if (mode is CombatSessionOverlayMode.Failure or CombatSessionOverlayMode.Routing)
            {
                return;
            }

            PauseTime();
            SetMode(CombatSessionOverlayMode.Pause);
        }

        public void ShowSettings()
        {
            if (mode is CombatSessionOverlayMode.Failure or CombatSessionOverlayMode.Routing)
            {
                return;
            }

            PauseTime();
            SetMode(CombatSessionOverlayMode.Settings);
        }

        public void ShowFailure()
        {
            if (mode == CombatSessionOverlayMode.Routing)
            {
                return;
            }

            RestoreTimeScale();
            SetMode(CombatSessionOverlayMode.Failure);
        }

        public void Resume()
        {
            if (mode is not (CombatSessionOverlayMode.Pause or CombatSessionOverlayMode.Settings))
            {
                return;
            }

            RestoreTimeScale();
            SetMode(CombatSessionOverlayMode.Hidden);
        }

        public void DismissForStageClear()
        {
            RestoreTimeScale();
            SetMode(CombatSessionOverlayMode.Hidden);
        }

        private void SetMode(CombatSessionOverlayMode newMode)
        {
            mode = newMode;
            bool visible = newMode != CombatSessionOverlayMode.Hidden;
            SetCanvasVisible(visible);
            PublishCombatInputBlock(visible);

            if (!visible)
            {
                SetRouteStatus(string.Empty);
                return;
            }

            if (newMode == CombatSessionOverlayMode.Routing)
            {
                SetRouteStatus("LOADING");
                SetActionsInteractable(false);
                return;
            }

            SetActionsInteractable(true);
            SetRouteStatus(string.Empty);
            ApplyModePresentation(newMode);
        }

        private void ApplyModePresentation(CombatSessionOverlayMode newMode)
        {
            bool pauseMode = newMode == CombatSessionOverlayMode.Pause;
            bool settingsMode = newMode == CombatSessionOverlayMode.Settings;
            bool failureMode = newMode == CombatSessionOverlayMode.Failure;

            SetActive(resumeButton, pauseMode || settingsMode);
            SetActive(retryButton, pauseMode || failureMode);
            SetActive(settingsButton, pauseMode);
            SetActive(stageSelectButton, pauseMode || failureMode);
            SetActive(lobbyButton, pauseMode || failureMode);
            SetActive(backButton, settingsMode);
            if (settingsContent != null)
            {
                settingsContent.SetActive(settingsMode);
            }

            if (pauseMode)
            {
                SetText(titleText, "PAUSED");
                SetText(bodyText, "COMBAT SUSPENDED");
                SetText(detailText, string.Empty);
                SetAccent(pauseAccent, pauseIcon);
                return;
            }

            if (settingsMode)
            {
                SetText(titleText, "SETTINGS");
                SetText(bodyText, "COMBAT PRESENTATION");
                SetText(detailText, string.Empty);
                SetAccent(settingsAccent, settingsIcon);
                SyncScreenCueToggle();
                return;
            }

            SetText(titleText, ResolveFailureTitle());
            SetText(bodyText, ResolveFailureSummary());
            SetText(detailText, BuildFailureDetails());
            SetAccent(failureAccent, null);
        }

        private void SetAccent(Color color, Sprite icon)
        {
            if (accentImage != null)
            {
                accentImage.color = color;
            }

            if (modeIcon != null)
            {
                modeIcon.sprite = icon;
                modeIcon.color = color;
                modeIcon.gameObject.SetActive(icon != null);
            }

            if (titleText != null)
            {
                titleText.color = color;
            }
        }

        private void AddButtonListeners()
        {
            resumeButton?.onClick.AddListener(Resume);
            retryButton?.onClick.AddListener(HandleRetryClicked);
            settingsButton?.onClick.AddListener(ShowSettings);
            stageSelectButton?.onClick.AddListener(HandleStageSelectClicked);
            lobbyButton?.onClick.AddListener(HandleLobbyClicked);
            backButton?.onClick.AddListener(ShowPause);
            screenCuesToggle?.onValueChanged.AddListener(HandleScreenCuesChanged);
        }

        private void RemoveButtonListeners()
        {
            resumeButton?.onClick.RemoveListener(Resume);
            retryButton?.onClick.RemoveListener(HandleRetryClicked);
            settingsButton?.onClick.RemoveListener(ShowSettings);
            stageSelectButton?.onClick.RemoveListener(HandleStageSelectClicked);
            lobbyButton?.onClick.RemoveListener(HandleLobbyClicked);
            backButton?.onClick.RemoveListener(ShowPause);
            screenCuesToggle?.onValueChanged.RemoveListener(HandleScreenCuesChanged);
        }

        private void HandleRetryClicked()
        {
            RequestRoute(retryRoute);
        }

        private void HandleStageSelectClicked()
        {
            RequestRoute(stageSelectRoute);
        }

        private void HandleLobbyClicked()
        {
            RequestRoute(lobbyRoute);
        }

        private void RequestRoute(UIRouteId routeId)
        {
            if (routingRoutine != null)
            {
                return;
            }

            if (routeTable == null || !routeTable.TryGetRoute(routeId, out UIScreenRouteTable.Route route))
            {
                SetRouteStatus("ROUTE UNAVAILABLE");
                return;
            }

            if (routeLoader == null)
            {
                SetRouteStatus("ROUTE LOADER MISSING");
                return;
            }

            modeBeforeRouting = mode;
            RestoreTimeScale();
            Time.timeScale = 1f;
            SetMode(CombatSessionOverlayMode.Routing);
            routingRoutine = StartCoroutine(LoadRouteRoutine(route));
        }

        private IEnumerator LoadRouteRoutine(UIScreenRouteTable.Route route)
        {
            string failure = null;
            yield return routeLoader.Load(route, null, reason => failure = reason);
            routingRoutine = null;

            if (string.IsNullOrWhiteSpace(failure))
            {
                yield break;
            }

            if (modeBeforeRouting is CombatSessionOverlayMode.Pause or CombatSessionOverlayMode.Settings)
            {
                PauseTime();
            }

            SetMode(modeBeforeRouting);
            SetRouteStatus("ROUTE FAILED");
        }

        private void HandleScreenCuesChanged(bool visible)
        {
            screenCuePresenter?.SetScreenCuesVisible(visible);
        }

        private void SyncScreenCueToggle()
        {
            if (screenCuesToggle == null)
            {
                return;
            }

            bool visible = screenCuePresenter == null || screenCuePresenter.ShowScreenCues;
            screenCuesToggle.SetIsOnWithoutNotify(visible);
        }

        private void EnablePauseInput()
        {
            if (pauseAction != null || pauseKey == Key.None)
            {
                return;
            }

            pauseAction = new InputAction("CombatPause", InputActionType.Button);
            pauseAction.AddBinding($"<Keyboard>/{pauseKey}");
            pauseAction.AddBinding("<Gamepad>/start");
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

        private void HandlePausePerformed(InputAction.CallbackContext _)
        {
            if (mode == CombatSessionOverlayMode.Hidden)
            {
                ShowPause();
            }
            else if (mode is CombatSessionOverlayMode.Pause or CombatSessionOverlayMode.Settings)
            {
                Resume();
            }
        }

        private void PauseTime()
        {
            if (hasPausedTime)
            {
                return;
            }

            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            hasPausedTime = true;
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

        private void PublishCombatInputBlock(bool blocked)
        {
            if (publishedInputBlock == blocked)
            {
                return;
            }

            publishedInputBlock = blocked;
            CombatInputBlockChanged?.Invoke(blocked);
        }

        private void SetCanvasVisible(bool visible)
        {
            for (int i = 0; i < managedLayoutGroups.Length; i++)
            {
                LayoutGroup layoutGroup = managedLayoutGroups[i];
                if (layoutGroup != null)
                {
                    layoutGroup.enabled = visible && authoredLayoutGroupStates[i];
                }
            }

            if (canvasGroup == null)
            {
                return;
            }

            if (visible)
            {
                transform.SetAsLastSibling();
            }

            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.interactable = visible;
            canvasGroup.blocksRaycasts = visible;
        }

        private void SetActionsInteractable(bool interactable)
        {
            SetInteractable(resumeButton, interactable);
            SetInteractable(retryButton, interactable);
            SetInteractable(settingsButton, interactable);
            SetInteractable(stageSelectButton, interactable);
            SetInteractable(lobbyButton, interactable);
            SetInteractable(backButton, interactable);
            if (screenCuesToggle != null)
            {
                screenCuesToggle.interactable = interactable;
            }
        }

        private void SetRouteStatus(string value)
        {
            SetText(routeStatusText, value);
            if (routeStatusText != null)
            {
                routeStatusText.gameObject.SetActive(!string.IsNullOrWhiteSpace(value));
            }
        }

        private string ResolveFailureTitle()
        {
            if (TryGetCommittedResult(out BossBarrageEncounterController.RouteResultRecord record)
                && !string.IsNullOrWhiteSpace(record.Title))
            {
                return record.Title;
            }

            return ResolveText(resultSource?.StageProfile?.FailTitle, "PLAYER DOWN");
        }

        private string ResolveFailureSummary()
        {
            if (TryGetCommittedResult(out BossBarrageEncounterController.RouteResultRecord record)
                && !string.IsNullOrWhiteSpace(record.Summary))
            {
                return record.Summary;
            }

            if (resultSource != null && resultSource.FailedFromRouteStabilityCollapse)
            {
                return ResolveText(
                    resultSource.StageProfile?.RouteCollapseFailDetail,
                    "Pressure control collapsed before the combat answer completed.");
            }

            return ResolveText(
                resultSource?.StageProfile?.FailDetail,
                "Player HP reached zero before the boss pressure was answered.");
        }

        private string BuildFailureDetails()
        {
            float elapsed = 0f;
            int completed = 0;
            int total = 0;
            if (TryGetCommittedResult(out BossBarrageEncounterController.RouteResultRecord record))
            {
                elapsed = record.ElapsedSeconds;
                completed = record.CompletedObjectiveStepCount;
                total = record.ObjectiveStepCount;
            }
            else if (resultSource != null)
            {
                elapsed = resultSource.ResultElapsedSeconds;
                completed = resultSource.CompletedObjectiveStepCount;
                total = resultSource.ObjectiveStepCount;
            }

            string survival = resultSource != null && resultSource.FailedFromRouteStabilityCollapse
                ? "Pressure control zero"
                : "Player down";
            string next = ResolveText(
                resultSource?.StageProfile?.FailedRouteNextObjective,
                "Protect HP first, then spend summon on the visible curtain.");
            return $"Time  {elapsed:0.0}s\nObjectives  {completed}/{total}\nSurvival  {survival}\nNext  {next}";
        }

        private bool TryGetCommittedResult(out BossBarrageEncounterController.RouteResultRecord record)
        {
            if (resultSource != null && resultSource.HasCommittedResultRecord)
            {
                record = resultSource.LastResultRecord;
                return true;
            }

            record = default;
            return false;
        }

        private static string ResolveText(string value, string fallback)
        {
            return string.IsNullOrWhiteSpace(value) ? fallback : value;
        }

        private static void SetText(Text target, string value)
        {
            if (target != null)
            {
                target.text = value ?? string.Empty;
            }
        }

        private static void SetActive(Button button, bool active)
        {
            if (button != null)
            {
                button.gameObject.SetActive(active);
            }
        }

        private static void SetInteractable(Selectable selectable, bool interactable)
        {
            if (selectable != null)
            {
                selectable.interactable = interactable;
            }
        }
    }
}
