using System.Collections;
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
    public sealed class OlympusStationCombatIntroTutorialBridge : MonoBehaviour
    {
        private const string TargetSceneName = "OlympusStationCombatStage";
        private const string SystemSpeaker = "천계관리시스템";
        private const string ReplicaGrantLine = "영혼 동기화율 70퍼센트, 전생특전 '레플리카'를 지급합니다.";
        private const string SummonGuideLine =
            "코스트 수치를 만족하면 소환수를 소환할 수 있습니다.";

        [SerializeField] private SceneEntryNoticeOverlay noticeOverlay;
        [SerializeField] private OlympusTutorialOverlayPresenter overlayPresenter;
        [SerializeField] private BossBarrageLaneReviewMobileHud mobileHud;
        [SerializeField, Min(0f)] private float minimumReadSeconds = 0.18f;
        [SerializeField, Min(0f)] private float confirmedFlashSeconds = 0.14f;
        [SerializeField] private string advanceInputLabel = "계속";
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
        }

        private IEnumerator PlayGuideBeforeGameplayRelease()
        {
            if (played || SceneManager.GetActiveScene().name != TargetSceneName)
            {
                yield break;
            }

            played = true;
            ResolveReferences();
            SetGameplayInputLocked(true);

            if (overlayPresenter == null)
            {
                SetGameplayInputLocked(false);
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
        }

        private IEnumerator WaitForAdvanceInput()
        {
            float startTime = Time.unscaledTime;
            while (Time.unscaledTime - startTime < minimumReadSeconds)
            {
                yield return null;
            }

            while (!WasAdvancePressedThisFrame())
            {
                yield return null;
            }
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
            ResolveMobileHud();
            if (mobileHud == null)
            {
                return new Vector2(0.89f, 0.54f);
            }

            Rect unionRect = mobileHud.SummonSlot1GuiRect;
            if (!IsValidRect(unionRect))
            {
                return new Vector2(0.89f, 0.54f);
            }

            if (!mobileHud.UseSingleSummonButton)
            {
                unionRect = Encapsulate(unionRect, mobileHud.SummonSlot2GuiRect);
                unionRect = Encapsulate(unionRect, mobileHud.SummonSlot3GuiRect);
            }

            return GuiPointToScreenAnchor(unionRect.center);
        }

        private void SetGameplayInputLocked(bool locked)
        {
            ResolveReferences();
            if (movement != null)
            {
                if (locked)
                {
                    movement.SetMoveInput(Vector2.zero);
                    movement.SetLookInput(Vector2.zero);
                    movement.SetCinematicMoveInputSpeedScale(0f);
                }
                else
                {
                    movement.ClearCinematicMoveInputSpeedScale();
                }
            }

            actionController?.SetCinematicInputLocked(locked);
            combatModeController?.SetCinematicInputLocked(locked);
            skill1Action?.SetCinematicInputLocked(locked);
            summonSlot1Action?.SetCinematicInputLocked(locked);
            rangedBasicAttackAction?.SetCinematicInputLocked(locked);
            mobileHud?.SetTutorialInputBlocked(locked);
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
                supportSummonActions[i]?.SetCinematicInputLocked(locked);
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
            ResolveMobileHud();
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

        private void ResolveMobileHud()
        {
            if (mobileHud == null)
            {
                mobileHud = FindFirstObjectByType<BossBarrageLaneReviewMobileHud>();
            }
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

        private static Vector2 GuiPointToScreenAnchor(Vector2 guiPoint)
        {
            if (Screen.width <= 0 || Screen.height <= 0)
            {
                return Vector2.zero;
            }

            return new Vector2(
                Mathf.Clamp01(guiPoint.x / Screen.width),
                Mathf.Clamp01(1f - guiPoint.y / Screen.height));
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
