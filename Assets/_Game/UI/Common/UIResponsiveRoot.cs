using UnityEngine;
using UnityEngine.UI;

namespace DimensionBrawl.UI
{
    [DisallowMultipleComponent]
    public sealed class UIResponsiveRoot : MonoBehaviour
    {
        [SerializeField] private UIResponsiveLayoutCatalog catalog;
        [SerializeField] private CanvasScaler canvasScaler;
        [SerializeField] private UISafeAreaRoot safeAreaRoot;
        [SerializeField] private Text breakpointText;
        [SerializeField] private bool applyCanvasScaler = true;

        private Vector2Int lastResolution = new Vector2Int(-1, -1);

        private void Reset()
        {
            canvasScaler = GetComponent<CanvasScaler>();
            safeAreaRoot = GetComponentInChildren<UISafeAreaRoot>();
        }

        private void OnEnable()
        {
            ApplyIfResolutionChanged(true);
        }

        private void Update()
        {
            ApplyIfResolutionChanged(false);
        }

        private void ApplyIfResolutionChanged(bool force)
        {
            Vector2Int resolution = new Vector2Int(Mathf.Max(1, Screen.width), Mathf.Max(1, Screen.height));
            if (!force && resolution == lastResolution)
            {
                return;
            }

            lastResolution = resolution;
            Apply(new Vector2(resolution.x, resolution.y));
        }

        private void Apply(Vector2 resolution)
        {
            if (catalog == null || !catalog.TryResolve(resolution, out UIResponsiveLayoutCatalog.BreakpointEntry entry))
            {
                if (breakpointText != null)
                {
                    breakpointText.text = "Layout";
                }

                return;
            }

            if (applyCanvasScaler && canvasScaler != null)
            {
                canvasScaler.referenceResolution = entry.ReferenceResolution;
                canvasScaler.matchWidthOrHeight = entry.MatchWidthOrHeight;
            }

            if (safeAreaRoot != null)
            {
                safeAreaRoot.ApplyLayout(entry.SafeAreaMode, entry.EdgeInset);
            }

            if (breakpointText != null)
            {
                breakpointText.text = entry.Id;
            }
        }
    }
}
