using UnityEngine;

namespace DimensionBrawl.Presentation
{
    [DisallowMultipleComponent]
    public sealed class ActionFoundationHovlRotator : MonoBehaviour
    {
        [SerializeField] private Vector3 eulerStep = new Vector3(0f, 1f, 0f);
        [SerializeField, Min(0.001f)] private float intervalSeconds = 0.0167f;

        public void Configure(Vector3 newEulerStep, float newIntervalSeconds)
        {
            eulerStep = newEulerStep;
            intervalSeconds = Mathf.Max(0.001f, newIntervalSeconds);
        }

        private void OnEnable()
        {
            InvokeRepeating(nameof(Rotate), 0f, intervalSeconds);
        }

        private void OnDisable()
        {
            CancelInvoke();
        }

        private void Rotate()
        {
            transform.localEulerAngles += eulerStep;
        }
    }
}
