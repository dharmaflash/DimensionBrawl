using DimensionBrawl.Player;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DimensionBrawl.UI
{
    [DisallowMultipleComponent]
    public sealed class CombatHudPointerActionInput : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, ISubmitHandler
    {
        private const int NoPointerId = int.MinValue;

        [SerializeField] private CombatHudInputBridge inputBridge;
        [SerializeField] private CombatHudActionId actionId = CombatHudActionId.None;
        [SerializeField] private bool sendHoldState;

        private bool pointerHeld;
        private PlayerInputLockSource inputBlockSources;
        private int activePointerId = NoPointerId;
        private Button visualButton;

        public CombatHudActionId ActionId => actionId;
        public bool SendsHoldState => sendHoldState;
        public bool IsInputBlocked => inputBlockSources != PlayerInputLockSource.None;
        public bool IsPointerHeld => pointerHeld;

        public void Configure(CombatHudInputBridge newInputBridge, CombatHudActionId newActionId, bool newSendHoldState = false)
        {
            inputBridge = newInputBridge;
            actionId = newActionId;
            sendHoldState = newSendHoldState;
        }

        private void Awake()
        {
            if (inputBridge == null)
            {
                inputBridge = GetComponentInParent<CombatHudInputBridge>();
            }

            CacheVisualButton();
        }

        private void OnEnable()
        {
            CacheVisualButton();
        }

        private void OnDisable()
        {
            ReleaseHold();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!CanAcceptPointer(eventData) || pointerHeld)
            {
                return;
            }

            pointerHeld = true;
            activePointerId = eventData.pointerId;
            if (sendHoldState)
            {
                inputBridge?.SetActionHeld(actionId, true);
            }

            inputBridge?.RequestAction(actionId);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!IsActivePointer(eventData))
            {
                return;
            }

            ReleaseHold();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (IsActivePointer(eventData))
            {
                ReleaseHold();
            }
        }

        public void OnSubmit(BaseEventData eventData)
        {
            if (CanRequestAction())
            {
                inputBridge?.RequestAction(actionId);
            }
        }

        public void SetInputBlocked(PlayerInputLockSource source, bool blocked)
        {
            bool wasBlocked = IsInputBlocked;
            inputBlockSources = PlayerInputLockMask.WithState(inputBlockSources, source, blocked);
            if (wasBlocked == IsInputBlocked)
            {
                return;
            }

            if (IsInputBlocked)
            {
                ReleaseHold();
            }
        }

        private void ReleaseHold()
        {
            if (!pointerHeld)
            {
                return;
            }

            pointerHeld = false;
            activePointerId = NoPointerId;
            if (sendHoldState && actionId != CombatHudActionId.None)
            {
                inputBridge?.SetActionHeld(actionId, false);
            }
        }

        private bool CanAcceptPointer(PointerEventData eventData)
        {
            return eventData != null
                && eventData.button == PointerEventData.InputButton.Left
                && CanRequestAction();
        }

        private bool CanRequestAction()
        {
            CacheVisualButton();
            return !IsInputBlocked
                && actionId != CombatHudActionId.None
                && (visualButton == null || visualButton.IsInteractable());
        }

        private bool IsActivePointer(PointerEventData eventData)
        {
            return pointerHeld
                && eventData != null
                && eventData.pointerId == activePointerId;
        }

        private void CacheVisualButton()
        {
            if (visualButton == null)
            {
                visualButton = GetComponent<Button>();
            }
        }
    }
}
