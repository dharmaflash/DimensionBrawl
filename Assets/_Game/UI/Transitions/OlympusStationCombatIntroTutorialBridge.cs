using System;
using System.Collections;
using DimensionBrawl.Combat;
using DimensionBrawl.Player;
using DimensionBrawl.Presentation;
using DimensionBrawl.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace DimensionBrawl.LevelDesign
{
    [DisallowMultipleComponent]
    public sealed class OlympusStationCombatIntroTutorialBridge : MonoBehaviour, ICombatEntryGuideGate
    {
        public enum CoreLoopCoachPhase
        {
            Inactive,
            AwaitingEncounter,
            SeekForwardRisk,
            HoldFrontline,
            AwaitingSummonBlock,
            AwaitingSkill1Hit,
            Completed,
            Interrupted
        }

        private const string SystemSpeaker = "천계관리시스템";
        private const string ReplicaGrantLine = "영혼 동기화율 70퍼센트, 전생특전 '레플리카'를 지급합니다.";
        private const string SummonGuideLine =
            "전진할수록 EN 충전이 빨라집니다. 보스 포화를 소환으로 막고 Skill1로 추격하십시오.";
        private const string ForwardRiskGuideLine =
            "전진 위험대에서 EN 충전이 가속됩니다. 안전 틈을 읽고 앞으로 진입하세요.";
        private const string HoldFrontlineGuideLine =
            "EN 가속 확인. 근접 위협을 정리해 보스 포화 차단 기회를 여세요.";
        private const string SummonBlockGuideLine =
            "포화 차단 기회. 소환 1로 투사체를 막아 반격 창을 여세요.";
        private const string Skill1FollowupGuideLine =
            "포화 차단 성공. 열린 반격 창에 Skill1을 명중시키세요.";
        private const string Skill1RetryGuideLine =
            "반격 창이 닫혔습니다. 다음 포화를 소환으로 다시 차단하세요.";
        private const string CoreLoopConfirmedLine =
            "연계 확인: 전진 충전 → 소환 차단 → Skill1 반격.";

        [SerializeField] private SceneEntryNoticeOverlay noticeOverlay;
        [SerializeField] private OlympusTutorialOverlayPresenter overlayPresenter;
        [SerializeField] private CombatHudAimDragInput combatHudAimDragInput;
        [SerializeField] private CombatHudVirtualJoystick combatHudMoveJoystick;
        [SerializeField] private CombatHudPointerActionInput[] combatHudPointerActions = Array.Empty<CombatHudPointerActionInput>();
        [SerializeField, Min(0f)] private float minimumReadSeconds = 0.18f;
        [SerializeField, Min(0f)] private float confirmedFlashSeconds = 0.14f;
        [SerializeField] private string advanceInputLabel = "계속";
        [Header("Core Loop Coach")]
        [SerializeField] private bool runCoreLoopCoachAfterRelease = true;
        [SerializeField, Min(1f)] private float coreCoachMaximumSeconds = 45f;
        [SerializeField] private SummonEnergyLadder energyLadder;
        [SerializeField] private BossBarrageEncounterController bossEncounter;
        [Header("Voice")]
        [SerializeField] private AudioSource voiceAudioSource;
        [SerializeField] private AudioClip replicaGrantVoiceClip;
        [SerializeField] private AudioClip summonGuideVoiceClip;
        [SerializeField, Range(0f, 1f)] private float voiceVolume = 1f;
        [SerializeField, Min(0f)] private float voiceDelaySeconds;

        private PlayerMovementController movement;
        private PlayerActionController actionController;
        private PlayerCombatModeController combatModeController;
        private PlayerRangedBasicAttackAction rangedBasicAttackAction;
        private PlayerSkill1Action skill1Action;
        private PlayerSummonSlot1Action summonSlot1Action;
        private PlayerSupportSummonSlotAction[] supportSummonActions;
        private bool played;
        private bool gameplayInputLocked;
        private bool awaitingAdvance;
        private bool advanceRequested;
        private bool coreCoachEventsSubscribed;
        private bool coreCoachObservedRunningEncounter;
        private bool forwardRiskObserved;
        private bool summonBlockOpportunityObserved;
        private bool summonFollowupWindowObserved;
        private bool summonFollowupHitObserved;
        private Coroutine coreCoachRoutine;

        public CombatEntryGuideState State { get; private set; } = CombatEntryGuideState.NotStarted;
        public bool IsGuidePlaying => State == CombatEntryGuideState.Playing;
        public bool IsAwaitingAdvance => awaitingAdvance;
        public bool IsCoreLoopCoachRunning => coreCoachRoutine != null;
        public CoreLoopCoachPhase CurrentCoreLoopCoachPhase { get; private set; } = CoreLoopCoachPhase.Inactive;

        public event Action<CombatEntryGuideState> StateChanged;
        public event Action<CoreLoopCoachPhase> CoreLoopCoachPhaseChanged;

        public void RequestAdvance()
        {
            if (awaitingAdvance)
            {
                advanceRequested = true;
            }
        }

        private void Reset()
        {
            noticeOverlay = GetComponent<SceneEntryNoticeOverlay>();
            overlayPresenter = GetComponent<OlympusTutorialOverlayPresenter>();
        }

        private void OnEnable()
        {
            ResolveNoticeOverlay();
            if (noticeOverlay != null)
            {
                noticeOverlay.BeforeGameplayPauseReleased += PlayGuideBeforeGameplayRelease;
            }
        }

        private void OnDisable()
        {
            if (noticeOverlay != null)
            {
                noticeOverlay.BeforeGameplayPauseReleased -= PlayGuideBeforeGameplayRelease;
            }

            if (gameplayInputLocked)
            {
                SetGameplayInputLocked(false);
            }

            if (State == CombatEntryGuideState.Playing)
            {
                SetGuideState(CombatEntryGuideState.Interrupted);
            }

            awaitingAdvance = false;
            advanceRequested = false;
            StopCoreLoopCoach(CoreLoopCoachPhase.Interrupted);
            overlayPresenter?.Hide();
        }

        private IEnumerator PlayGuideBeforeGameplayRelease()
        {
            StageRunContext context = StageRunRuntime.ActiveContext;
            if (played
                || context == null
                || context.LifecycleState != StageRunLifecycleState.StationActive
                || context.CurrentSceneHandle != gameObject.scene.handle)
            {
                yield break;
            }

            played = true;
            ResolveReferences();
            SetGameplayInputLocked(true);
            SetGuideState(CombatEntryGuideState.Playing);

            if (overlayPresenter == null)
            {
                SetGameplayInputLocked(false);
                SetGuideState(CombatEntryGuideState.Interrupted);
                yield break;
            }

            overlayPresenter.SetGuideProgress(1, 2, "READ");
            overlayPresenter.Show(
                SystemSpeaker,
                ReplicaGrantLine,
                advanceInputLabel,
                OlympusTutorialOverlayPresenter.FocusKind.None,
                new Vector2(0.24f, 0.43f),
                replicaGrantVoiceClip,
                voiceVolume,
                voiceDelaySeconds);
            yield return WaitForAdvanceInput();
            overlayPresenter.SetGuideState(OlympusTutorialOverlayPresenter.GuideState.Confirmed);
            yield return WaitRealtime(confirmedFlashSeconds);

            overlayPresenter.SetGuideProgress(2, 2, "ACT");
            overlayPresenter.Show(
                SystemSpeaker,
                SummonGuideLine,
                advanceInputLabel,
                OlympusTutorialOverlayPresenter.FocusKind.SummonSlots,
                ResolveSummonSlotsAnchor(),
                summonGuideVoiceClip,
                voiceVolume,
                voiceDelaySeconds);
            overlayPresenter.SetGuideState(OlympusTutorialOverlayPresenter.GuideState.Ready);
            yield return WaitForAdvanceInput();
            overlayPresenter.SetGuideState(OlympusTutorialOverlayPresenter.GuideState.Confirmed);
            yield return WaitRealtime(confirmedFlashSeconds);

            overlayPresenter.Hide();
            SetGameplayInputLocked(false);
            SetGuideState(CombatEntryGuideState.Released);
            StartCoreLoopCoachAfterRelease();
        }

        private void StartCoreLoopCoachAfterRelease()
        {
            if (!runCoreLoopCoachAfterRelease
                || !isActiveAndEnabled
                || State != CombatEntryGuideState.Released
                || coreCoachRoutine != null)
            {
                return;
            }

            ResolveReferences();
            ResolveCoreCoachReferences();
            if (overlayPresenter == null || energyLadder == null || bossEncounter == null)
            {
                SetCoreLoopCoachPhase(CoreLoopCoachPhase.Interrupted);
                return;
            }

            coreCoachRoutine = StartCoroutine(RunCoreLoopCoachAfterRelease());
        }

        private IEnumerator RunCoreLoopCoachAfterRelease()
        {
            ResetCoreLoopCoachObservations();
            SubscribeCoreLoopCoachEvents();
            SetCoreLoopCoachPhase(CoreLoopCoachPhase.AwaitingEncounter);
            float deadline = Time.unscaledTime + Mathf.Max(1f, coreCoachMaximumSeconds);

            while (Time.unscaledTime < deadline)
            {
                if (!coreCoachObservedRunningEncounter)
                {
                    if (Time.timeScale <= 0f)
                    {
                        yield return null;
                        continue;
                    }

                    if (!bossEncounter.IsRunning)
                    {
                        if (bossEncounter.Skill1FollowupHitConfirmed)
                        {
                            coreCoachObservedRunningEncounter = true;
                            CaptureCoreLoopSnapshot();
                        }
                        else if (bossEncounter.IsCleared || bossEncounter.IsFailed)
                        {
                            SetCoreLoopCoachPhase(CoreLoopCoachPhase.Interrupted);
                            break;
                        }
                        else
                        {
                            yield return null;
                            continue;
                        }
                    }

                    if (coreCoachObservedRunningEncounter)
                    {
                        continue;
                    }

                    coreCoachObservedRunningEncounter = true;
                    CaptureCoreLoopSnapshot();
                }

                if (CurrentCoreLoopCoachPhase == CoreLoopCoachPhase.Completed)
                {
                    overlayPresenter.SetGuideState(OlympusTutorialOverlayPresenter.GuideState.Confirmed);
                    yield return WaitRealtime(confirmedFlashSeconds);
                    break;
                }

                if (coreCoachObservedRunningEncounter && !bossEncounter.IsRunning)
                {
                    SetCoreLoopCoachPhase(CoreLoopCoachPhase.Interrupted);
                    break;
                }

                yield return null;
            }

            if (CurrentCoreLoopCoachPhase != CoreLoopCoachPhase.Completed)
            {
                SetCoreLoopCoachPhase(CoreLoopCoachPhase.Interrupted);
            }

            UnsubscribeCoreLoopCoachEvents();
            overlayPresenter?.Hide();
            coreCoachRoutine = null;
        }

        private void CaptureRiskBand(SummonEnergyRiskBand riskBand)
        {
            if (riskBand == SummonEnergyRiskBand.ForwardRisk)
            {
                CaptureForwardRisk();
            }
        }

        private void CaptureForwardRisk()
        {
            if (!coreCoachObservedRunningEncounter || forwardRiskObserved)
            {
                return;
            }

            forwardRiskObserved = true;
            RefreshCoreLoopCoachPresentation();
        }

        private void CaptureSummonBlockOpportunity()
        {
            summonBlockOpportunityObserved = true;
            RefreshCoreLoopCoachPresentation();
        }

        private void CaptureSummonFollowupWindow(int tier)
        {
            summonBlockOpportunityObserved = true;
            summonFollowupWindowObserved = true;
            RefreshCoreLoopCoachPresentation();
        }

        private void CaptureSummonFollowupMiss()
        {
            if (summonFollowupHitObserved)
            {
                return;
            }

            summonFollowupWindowObserved = false;
            RefreshCoreLoopCoachPresentation(Skill1RetryGuideLine);
        }

        private void CaptureSummonFollowupHit(int tier, float damage)
        {
            if (damage <= 0f)
            {
                return;
            }

            summonBlockOpportunityObserved = true;
            summonFollowupWindowObserved = true;
            summonFollowupHitObserved = true;
            RefreshCoreLoopCoachPresentation();
        }

        private void CaptureCoreLoopSnapshot()
        {
            forwardRiskObserved |= energyLadder.CurrentRiskBand == SummonEnergyRiskBand.ForwardRisk;
            summonBlockOpportunityObserved |= bossEncounter.CloseThreatDefeated
                || bossEncounter.BlockedBossPressureWithSummon
                || bossEncounter.IsSummonFollowupWindowActive
                || bossEncounter.Skill1FollowupHitConfirmed;
            summonFollowupWindowObserved |= bossEncounter.IsSummonFollowupWindowActive;
            summonFollowupHitObserved |= bossEncounter.Skill1FollowupHitConfirmed;
            RefreshCoreLoopCoachPresentation();
        }

        private void RefreshCoreLoopCoachPresentation(string summonMessage = SummonBlockGuideLine)
        {
            if (!coreCoachObservedRunningEncounter
                || CurrentCoreLoopCoachPhase == CoreLoopCoachPhase.Completed)
            {
                return;
            }

            if (!forwardRiskObserved)
            {
                SetCoreLoopCoachPhase(CoreLoopCoachPhase.SeekForwardRisk);
                ShowCoreLoopCoachCue(
                    ForwardRiskGuideLine,
                    "전진",
                    OlympusTutorialOverlayPresenter.FocusKind.MoveStick,
                    ResolveMoveAnchor());
                return;
            }

            if (summonFollowupHitObserved)
            {
                PresentCoreLoopCompletion();
                return;
            }

            if (summonFollowupWindowObserved)
            {
                PresentSkill1FollowupCue();
                return;
            }

            if (summonBlockOpportunityObserved)
            {
                PresentSummonBlockCue(summonMessage);
                return;
            }

            SetCoreLoopCoachPhase(CoreLoopCoachPhase.HoldFrontline);
            ShowCoreLoopCoachCue(
                HoldFrontlineGuideLine,
                "전선 유지",
                OlympusTutorialOverlayPresenter.FocusKind.RangedAttack,
                ResolveActionAnchor(CombatHudActionId.BasicAttack, new Vector2(0.91f, 0.76f)));
        }

        private void PresentCoreLoopCompletion()
        {
            SetCoreLoopCoachPhase(CoreLoopCoachPhase.Completed);
            ShowCoreLoopCoachCue(
                CoreLoopConfirmedLine,
                "연계 확인",
                OlympusTutorialOverlayPresenter.FocusKind.Skill1,
                ResolveActionAnchor(CombatHudActionId.Skill1, new Vector2(0.82f, 0.80f)));
        }

        private void PresentSummonBlockCue(string message)
        {
            SetCoreLoopCoachPhase(CoreLoopCoachPhase.AwaitingSummonBlock);
            ShowCoreLoopCoachCue(
                message,
                "소환 1",
                OlympusTutorialOverlayPresenter.FocusKind.SummonSlots,
                ResolveActionAnchor(CombatHudActionId.SummonSlot1, new Vector2(0.89f, 0.54f)));
        }

        private void PresentSkill1FollowupCue()
        {
            SetCoreLoopCoachPhase(CoreLoopCoachPhase.AwaitingSkill1Hit);
            ShowCoreLoopCoachCue(
                Skill1FollowupGuideLine,
                "SKILL 1",
                OlympusTutorialOverlayPresenter.FocusKind.Skill1,
                ResolveActionAnchor(CombatHudActionId.Skill1, new Vector2(0.82f, 0.80f)));
        }

        private void ShowCoreLoopCoachCue(
            string message,
            string actionLabel,
            OlympusTutorialOverlayPresenter.FocusKind targetFocus,
            Vector2 targetAnchor)
        {
            if (overlayPresenter == null)
            {
                return;
            }

            overlayPresenter.SetGuideProgress(0, 0, "");
            overlayPresenter.Show(SystemSpeaker, message, actionLabel, targetFocus, targetAnchor);
            overlayPresenter.SetGuideState(OlympusTutorialOverlayPresenter.GuideState.Ready);
        }

        private void SetCoreLoopCoachPhase(CoreLoopCoachPhase nextPhase)
        {
            if (CurrentCoreLoopCoachPhase == nextPhase)
            {
                return;
            }

            CurrentCoreLoopCoachPhase = nextPhase;
            CoreLoopCoachPhaseChanged?.Invoke(nextPhase);
        }

        private void ResetCoreLoopCoachObservations()
        {
            coreCoachObservedRunningEncounter = false;
            forwardRiskObserved = false;
            summonBlockOpportunityObserved = false;
            summonFollowupWindowObserved = false;
            summonFollowupHitObserved = false;
        }

        private void ResolveCoreCoachReferences()
        {
            energyLadder ??= FindFirstObjectByType<SummonEnergyLadder>(FindObjectsInactive.Include);
            bossEncounter ??= FindFirstObjectByType<BossBarrageEncounterController>(FindObjectsInactive.Include);
        }

        private void SubscribeCoreLoopCoachEvents()
        {
            if (coreCoachEventsSubscribed || energyLadder == null || bossEncounter == null)
            {
                return;
            }

            energyLadder.RiskBandChanged += CaptureRiskBand;
            bossEncounter.SummonBlockOpportunityOpened += CaptureSummonBlockOpportunity;
            bossEncounter.SummonFollowupWindowOpened += CaptureSummonFollowupWindow;
            bossEncounter.SummonFollowupMissed += CaptureSummonFollowupMiss;
            bossEncounter.SummonFollowupHitConfirmed += CaptureSummonFollowupHit;
            coreCoachEventsSubscribed = true;
        }

        private void UnsubscribeCoreLoopCoachEvents()
        {
            if (!coreCoachEventsSubscribed)
            {
                return;
            }

            if (energyLadder != null)
            {
                energyLadder.RiskBandChanged -= CaptureRiskBand;
            }

            if (bossEncounter != null)
            {
                bossEncounter.SummonBlockOpportunityOpened -= CaptureSummonBlockOpportunity;
                bossEncounter.SummonFollowupWindowOpened -= CaptureSummonFollowupWindow;
                bossEncounter.SummonFollowupMissed -= CaptureSummonFollowupMiss;
                bossEncounter.SummonFollowupHitConfirmed -= CaptureSummonFollowupHit;
            }

            coreCoachEventsSubscribed = false;
        }

        private void StopCoreLoopCoach(CoreLoopCoachPhase finalPhase)
        {
            if (coreCoachRoutine != null)
            {
                StopCoroutine(coreCoachRoutine);
                coreCoachRoutine = null;
            }

            UnsubscribeCoreLoopCoachEvents();
            if (CurrentCoreLoopCoachPhase != CoreLoopCoachPhase.Inactive
                && CurrentCoreLoopCoachPhase != CoreLoopCoachPhase.Completed)
            {
                SetCoreLoopCoachPhase(finalPhase);
            }
        }

        private void SetGuideState(CombatEntryGuideState nextState)
        {
            if (State == nextState)
            {
                return;
            }

            State = nextState;
            StateChanged?.Invoke(nextState);
        }

        private IEnumerator WaitForAdvanceInput()
        {
            awaitingAdvance = false;
            advanceRequested = false;
            float startTime = Time.unscaledTime;
            while (Time.unscaledTime - startTime < minimumReadSeconds)
            {
                yield return null;
            }

            awaitingAdvance = true;
            while (!advanceRequested && !WasAdvancePressedThisFrame())
            {
                yield return null;
            }

            awaitingAdvance = false;
            advanceRequested = false;
        }

        private static IEnumerator WaitRealtime(float seconds)
        {
            if (seconds <= 0f)
            {
                yield break;
            }

            yield return new WaitForSecondsRealtime(seconds);
        }

        private Vector2 ResolveSummonSlotsAnchor()
        {
            ResolveCombatHudInputs();
            bool hasBounds = false;
            Rect screenBounds = default;
            for (int i = 0; i < combatHudPointerActions.Length; i++)
            {
                CombatHudPointerActionInput pointerAction = combatHudPointerActions[i];
                if (pointerAction == null || !IsSummonAction(pointerAction.ActionId))
                {
                    continue;
                }

                if (!TryGetScreenRect(pointerAction.transform as RectTransform, out Rect screenRect))
                {
                    continue;
                }

                screenBounds = hasBounds ? Encapsulate(screenBounds, screenRect) : screenRect;
                hasBounds = true;
            }

            if (!hasBounds || Screen.width <= 0 || Screen.height <= 0)
            {
                return new Vector2(0.89f, 0.54f);
            }

            return new Vector2(
                Mathf.Clamp01(screenBounds.center.x / Screen.width),
                Mathf.Clamp01(screenBounds.center.y / Screen.height));
        }

        private Vector2 ResolveMoveAnchor()
        {
            ResolveCombatHudInputs();
            if (combatHudMoveJoystick != null
                && TryGetScreenRect(combatHudMoveJoystick.transform as RectTransform, out Rect screenRect)
                && Screen.width > 0
                && Screen.height > 0)
            {
                return new Vector2(
                    Mathf.Clamp01(screenRect.center.x / Screen.width),
                    Mathf.Clamp01(screenRect.center.y / Screen.height));
            }

            return new Vector2(0.13f, 0.76f);
        }

        private Vector2 ResolveActionAnchor(CombatHudActionId actionId, Vector2 fallbackAnchor)
        {
            ResolveCombatHudInputs();
            for (int i = 0; i < combatHudPointerActions.Length; i++)
            {
                CombatHudPointerActionInput pointerAction = combatHudPointerActions[i];
                if (pointerAction == null
                    || pointerAction.ActionId != actionId
                    || !TryGetScreenRect(pointerAction.transform as RectTransform, out Rect screenRect))
                {
                    continue;
                }

                if (Screen.width > 0 && Screen.height > 0)
                {
                    return new Vector2(
                        Mathf.Clamp01(screenRect.center.x / Screen.width),
                        Mathf.Clamp01(screenRect.center.y / Screen.height));
                }
            }

            return fallbackAnchor;
        }

        private void SetGameplayInputLocked(bool locked)
        {
            ResolveReferences();
            gameplayInputLocked = locked;
            if (movement != null)
            {
                if (locked)
                {
                    movement.SetMoveInput(Vector2.zero);
                    movement.SetLookInput(Vector2.zero);
                    movement.SetCinematicMoveInputLocked(PlayerInputLockSource.StationEntryGuide, true);
                }
                else
                {
                    movement.SetCinematicMoveInputLocked(PlayerInputLockSource.StationEntryGuide, false);
                }
            }

            actionController?.SetCinematicInputLocked(PlayerInputLockSource.StationEntryGuide, locked);
            combatModeController?.SetCinematicInputLocked(PlayerInputLockSource.StationEntryGuide, locked);
            skill1Action?.SetCinematicInputLocked(PlayerInputLockSource.StationEntryGuide, locked);
            summonSlot1Action?.SetCinematicInputLocked(PlayerInputLockSource.StationEntryGuide, locked);
            rangedBasicAttackAction?.SetCinematicInputLocked(PlayerInputLockSource.StationEntryGuide, locked);
            combatHudAimDragInput?.SetInputBlocked(PlayerInputLockSource.StationEntryGuide, locked);
            combatHudMoveJoystick?.SetInputBlocked(PlayerInputLockSource.StationEntryGuide, locked);
            for (int i = 0; i < combatHudPointerActions.Length; i++)
            {
                combatHudPointerActions[i]?.SetInputBlocked(PlayerInputLockSource.StationEntryGuide, locked);
            }

            if (locked && rangedBasicAttackAction != null)
            {
                rangedBasicAttackAction.SetFireHeld(false);
                rangedBasicAttackAction.ClearAimInput();
            }

            if (supportSummonActions == null)
            {
                return;
            }

            for (int i = 0; i < supportSummonActions.Length; i++)
            {
                supportSummonActions[i]?.SetCinematicInputLocked(
                    PlayerInputLockSource.StationEntryGuide,
                    locked);
            }
        }

        private void ResolveReferences()
        {
            ResolveNoticeOverlay();
            if (overlayPresenter == null)
            {
                overlayPresenter = GetComponent<OlympusTutorialOverlayPresenter>();
            }

            if (overlayPresenter == null && Application.isPlaying)
            {
                overlayPresenter = gameObject.AddComponent<OlympusTutorialOverlayPresenter>();
            }

            overlayPresenter?.ConfigureCommunicatorAudio(ResolveVoiceAudioSource(), null, 0f);
            ResolveCombatHudInputs();
            movement ??= FindFirstObjectByType<PlayerMovementController>();
            if (movement != null)
            {
                actionController ??= movement.GetComponent<PlayerActionController>();
                combatModeController ??= movement.GetComponent<PlayerCombatModeController>();
                rangedBasicAttackAction ??= movement.GetComponent<PlayerRangedBasicAttackAction>();
                skill1Action ??= movement.GetComponent<PlayerSkill1Action>();
                summonSlot1Action ??= movement.GetComponent<PlayerSummonSlot1Action>();
            }

            supportSummonActions ??= FindObjectsByType<PlayerSupportSummonSlotAction>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
        }

        private void ResolveCombatHudInputs()
        {
            combatHudAimDragInput ??= FindFirstObjectByType<CombatHudAimDragInput>(FindObjectsInactive.Include);
            combatHudMoveJoystick ??= FindFirstObjectByType<CombatHudVirtualJoystick>(FindObjectsInactive.Include);
            if (combatHudPointerActions == null || combatHudPointerActions.Length == 0)
            {
                combatHudPointerActions = FindObjectsByType<CombatHudPointerActionInput>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None);
            }
        }

        private void ResolveNoticeOverlay()
        {
            if (noticeOverlay == null)
            {
                noticeOverlay = GetComponent<SceneEntryNoticeOverlay>();
            }
        }

        private AudioSource ResolveVoiceAudioSource()
        {
            if (voiceAudioSource == null)
            {
                voiceAudioSource = GetComponent<AudioSource>();
            }

            if (voiceAudioSource == null)
            {
                voiceAudioSource = gameObject.AddComponent<AudioSource>();
            }

            voiceAudioSource.playOnAwake = false;
            voiceAudioSource.loop = false;
            voiceAudioSource.spatialBlend = 0f;
            return voiceAudioSource;
        }

        private static bool IsSummonAction(CombatHudActionId actionId)
        {
            return actionId == CombatHudActionId.SummonSlot1
                || actionId == CombatHudActionId.SummonSlot2
                || actionId == CombatHudActionId.SummonSlot3;
        }

        private static bool WasAdvancePressedThisFrame()
        {
#if ENABLE_INPUT_SYSTEM
            if (Touchscreen.current != null)
            {
                foreach (var touch in Touchscreen.current.touches)
                {
                    if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began)
                    {
                        return true;
                    }
                }
            }

            if (Mouse.current != null
                && (Mouse.current.leftButton.wasPressedThisFrame
                    || Mouse.current.rightButton.wasPressedThisFrame))
            {
                return true;
            }

            if (Keyboard.current != null
                && (Keyboard.current.spaceKey.wasPressedThisFrame
                    || Keyboard.current.enterKey.wasPressedThisFrame
                    || Keyboard.current.numpadEnterKey.wasPressedThisFrame))
            {
                return true;
            }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
            {
                return true;
            }

            for (int i = 0; i < Input.touchCount; i++)
            {
                if (Input.GetTouch(i).phase == TouchPhase.Began)
                {
                    return true;
                }
            }
#endif

            return false;
        }

        private static bool TryGetScreenRect(RectTransform rectTransform, out Rect screenRect)
        {
            if (rectTransform == null || !rectTransform.gameObject.activeInHierarchy)
            {
                screenRect = default;
                return false;
            }

            Canvas canvas = rectTransform.GetComponentInParent<Canvas>();
            Camera eventCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? canvas.worldCamera
                : null;
            Vector3[] corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);
            Vector2 minimum = RectTransformUtility.WorldToScreenPoint(eventCamera, corners[0]);
            Vector2 maximum = minimum;
            for (int i = 1; i < corners.Length; i++)
            {
                Vector2 point = RectTransformUtility.WorldToScreenPoint(eventCamera, corners[i]);
                minimum = Vector2.Min(minimum, point);
                maximum = Vector2.Max(maximum, point);
            }

            screenRect = Rect.MinMaxRect(minimum.x, minimum.y, maximum.x, maximum.y);
            return screenRect.width > 0.01f && screenRect.height > 0.01f;
        }

        private static Rect Encapsulate(Rect rect, Rect other)
        {
            if (!IsValidRect(other))
            {
                return rect;
            }

            return Rect.MinMaxRect(
                Mathf.Min(rect.xMin, other.xMin),
                Mathf.Min(rect.yMin, other.yMin),
                Mathf.Max(rect.xMax, other.xMax),
                Mathf.Max(rect.yMax, other.yMax));
        }

        private static bool IsValidRect(Rect rect)
        {
            return rect.width > 0.01f && rect.height > 0.01f;
        }
    }
}
