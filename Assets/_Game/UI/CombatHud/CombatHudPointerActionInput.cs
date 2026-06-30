using UnityEngine;
using UnityEngine.EventSystems;

namespace DimensionBrawl.UI
{
    [DisallowMultipleComponent]
    public sealed class CombatHudPointerActionInput : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        [SerializeField] private CombatHudInputBridge inputBridge;
        [SerializeField] private CombatHudActionId actionId = CombatHudActionId.None;
        [SerializeField] private bool sendHoldState;

        private bool pointerHeld;

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

            if (!sendHoldState)
            {
                return;
            }

            pointerHeld = true;
            inputBridge?.SetActionHeld(actionId, true);
            inputBridge?.RequestAction(actionId);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            ReleaseHold();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            ReleaseHold();
        }

        private void ReleaseHold()
        {
            if (!pointerHeld || !sendHoldState || actionId == CombatHudActionId.None)
            {
                return;
            }

            pointerHeld = false;
            inputBridge?.SetActionHeld(actionId, false);
        }
    }
}
