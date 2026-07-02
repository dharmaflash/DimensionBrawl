using UnityEngine;

namespace DimensionBrawl.UI
{
    [DisallowMultipleComponent]
    public sealed class UILoadingSpinner : MonoBehaviour
    {
        [SerializeField] private RectTransform target;
        [SerializeField] private float degreesPerSecond = -180f;

        private void Reset()
        {
            target = GetComponent<RectTransform>();
        }

        private void Awake()
        {
            if (target == null)
            {
                target = GetComponent<RectTransform>();
            }
        }

        private void OnEnable()
        {
            if (target != null)
            {
                target.localRotation = Quaternion.identity;
            }
        }

        private void Update()
        {
            if (target == null || Mathf.Approximately(degreesPerSecond, 0f))
            {
                return;
            }

            target.Rotate(0f, 0f, degreesPerSecond * Time.unscaledDeltaTime);
        }
    }
}
