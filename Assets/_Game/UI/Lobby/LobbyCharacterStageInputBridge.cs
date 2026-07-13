using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DimensionBrawl.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Graphic))]
    public sealed class LobbyCharacterStageInputBridge : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        private const int NoPointerId = int.MinValue;

        [SerializeField] private LobbyCharacterStageInputChannel inputChannel;
        [SerializeField, Min(0f)] private float tapMaxDragPixels = 18f;
        [SerializeField, Min(0f)] private float dragDeadZonePixels = 2f;

        private Vector2 pointerDownPosition;
        private float dragDistance;
        private bool isPointerHeld;
        private int activePointerId = NoPointerId;

        private void Reset()
        {
            Graphic graphic = GetComponent<Graphic>();
            if (graphic != null)
            {
                graphic.raycastTarget = true;
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData == null
                || eventData.button != PointerEventData.InputButton.Left
                || isPointerHeld)
            {
                return;
            }

            pointerDownPosition = eventData.position;
            dragDistance = 0f;
            isPointerHeld = true;
            activePointerId = eventData.pointerId;

            inputChannel?.RaiseBeginInteraction();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!IsActivePointer(eventData) || inputChannel == null)
            {
                return;
            }

            dragDistance = Vector2.Distance(pointerDownPosition, eventData.position);
            if (eventData.delta.sqrMagnitude < dragDeadZonePixels * dragDeadZonePixels)
            {
                return;
            }

            inputChannel.RaiseHorizontalDrag(eventData.delta.x);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!IsActivePointer(eventData))
            {
                return;
            }

            EndInteraction(allowTap: true);
        }

        private void OnDisable()
        {
            EndInteraction(allowTap: false);
        }

        private void EndInteraction(bool allowTap)
        {
            bool wasHeld = isPointerHeld;
            isPointerHeld = false;
            activePointerId = NoPointerId;

            if (!wasHeld || inputChannel == null)
            {
                return;
            }

            inputChannel.RaiseEndInteraction();
            if (allowTap && dragDistance <= tapMaxDragPixels)
            {
                inputChannel.RaiseTap();
            }
        }

        private bool IsActivePointer(PointerEventData eventData)
        {
            return isPointerHeld
                && eventData != null
                && eventData.pointerId == activePointerId;
        }
    }
}
