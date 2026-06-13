using UnityEngine;

namespace DimensionBrawl.UI
{
    [DisallowMultipleComponent]
    public sealed class UILayerPresenter : MonoBehaviour
    {
        [SerializeField] private UILayerCatalog catalog;
        [SerializeField] private UILayerId layerId = UILayerId.Screen;
        [SerializeField] private Canvas canvas;
        [SerializeField] private CanvasGroup canvasGroup;

        private void Reset()
        {
            canvas = GetComponent<Canvas>();
            canvasGroup = GetComponent<CanvasGroup>();
        }

        private void OnEnable()
        {
            Apply();
        }

        public void Apply()
        {
            if (catalog == null || !catalog.TryGetLayer(layerId, out UILayerCatalog.LayerEntry layer))
            {
                return;
            }

            if (canvas != null)
            {
                canvas.overrideSorting = true;
                canvas.sortingLayerName = layer.SortingLayerName;
                canvas.sortingOrder = layer.SortingOrder;
            }

            if (canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = layer.BlocksRaycasts;
            }
        }
    }
}
