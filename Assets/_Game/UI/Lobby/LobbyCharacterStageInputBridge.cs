using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DimensionBrawl.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Graphic))]
    public sealed class LobbyCharacterStageInputBridge : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField] private LobbyCharacterStageInputChannel inputChannel;
        [SerializeField, Min(0f)] private float tapMaxDragPixels = 18f;
        [SerializeField, Min(0f)] private float dragDeadZonePixels = 2f;

        private Vector2 pointerDownPosition;
        private float dragDistance;
        private bool isPointerHeld;

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
            pointerDownPosition = eventData.position;
            dragDistance = 0f;
            isPointerHeld = true;

            inputChannel?.RaiseBeginInteraction();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!isPointerHeld || inputChannel == null)
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
            if (!isPointerHeld)
            {
                return;
            }

            isPointerHeld = false;

            if (inputChannel == null)
            {
                return;
            }

            inputChannel.RaiseEndInteraction();
            if (dragDistance <= tapMaxDragPixels)
            {
                inputChannel.RaiseTap();
            }
        }
    }
}
