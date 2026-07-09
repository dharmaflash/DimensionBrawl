using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DimensionBrawl.UI
{
    [DisallowMultipleComponent]
    public sealed class CombatHudPointerActionInput : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        [SerializeField] private CombatHudInputBridge inputBridge;
        [SerializeField] private CombatHudActionId actionId = CombatHudActionId.None;
        [SerializeField] private bool sendHoldState;

        private bool pointerHeld;
        private Button visualButton;

        public CombatHudActionId ActionId => actionId;
        public bool SendsHoldState => sendHoldState;

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
            if (actionId == CombatHudActionId.None)
            {
                return;
            }

            pointerHeld = true;
            if (sendHoldState)
            {
                inputBridge?.SetActionHeld(actionId, true);
            }

            inputBridge?.RequestAction(actionId);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            ReleaseHold();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!sendHoldState)
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
            if (sendHoldState && actionId != CombatHudActionId.None)
            {
                inputBridge?.SetActionHeld(actionId, false);
            }
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
