using UnityEngine;

namespace DimensionBrawl.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public sealed class UISafeAreaRoot : MonoBehaviour
    {
        [SerializeField] private bool applyOnEnable = true;
        [SerializeField] private RectTransform target;
        [SerializeField] private UISafeAreaMode mode = UISafeAreaMode.InsetsOnly;
        [SerializeField, Min(0f)] private float extraInsetPixels;

        private Rect lastSafeArea;
        private Vector2Int lastResolution = new Vector2Int(-1, -1);

        private void Reset()
        {
            target = GetComponent<RectTransform>();
        }

        private void OnEnable()
        {
            if (applyOnEnable)
            {
                ApplySafeArea();
            }
        }

        private void Update()
        {
            Vector2Int resolution = new Vector2Int(Screen.width, Screen.height);
            if (Screen.safeArea != lastSafeArea || resolution != lastResolution)
            {
                ApplySafeArea();
            }
        }

        public void ApplyLayout(UISafeAreaMode newMode, float newExtraInsetPixels)
        {
            mode = newMode;
            extraInsetPixels = Mathf.Max(0f, newExtraInsetPixels);
            ApplySafeArea();
        }

        public void ApplySafeArea()
        {
            if (target == null)
            {
                target = GetComponent<RectTransform>();
            }

            float width = Mathf.Max(1f, Screen.width);
            float height = Mathf.Max(1f, Screen.height);
            Rect rawSafeArea = Screen.safeArea;
            Rect safeArea = mode == UISafeAreaMode.InsetsOnly
                ? rawSafeArea
                : new Rect(0f, 0f, width, height);

            if (extraInsetPixels > 0f)
            {
                float horizontalInset = Mathf.Min(extraInsetPixels, safeArea.width * 0.45f);
                float verticalInset = Mathf.Min(extraInsetPixels, safeArea.height * 0.45f);
                safeArea.xMin += horizontalInset;
                safeArea.xMax -= horizontalInset;
                safeArea.yMin += verticalInset;
                safeArea.yMax -= verticalInset;
            }

            lastSafeArea = rawSafeArea;
            lastResolution = new Vector2Int(Screen.width, Screen.height);
            Vector2 anchorMin = safeArea.position;
            Vector2 anchorMax = safeArea.position + safeArea.size;
            anchorMin.x /= width;
            anchorMin.y /= height;
            anchorMax.x /= width;
            anchorMax.y /= height;

            target.anchorMin = anchorMin;
            target.anchorMax = anchorMax;
            target.offsetMin = Vector2.zero;
            target.offsetMax = Vector2.zero;
        }
    }
}
