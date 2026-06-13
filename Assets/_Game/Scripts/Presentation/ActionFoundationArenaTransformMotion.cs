using UnityEngine;

namespace DimensionBrawl.Presentation
{
    [DisallowMultipleComponent]
    public sealed class ActionFoundationArenaTransformMotion : MonoBehaviour
    {
        [SerializeField] private Vector3 localRotationDegreesPerSecond = new Vector3(0f, 6f, 0f);
        [SerializeField] private Vector3 bobAxis = Vector3.up;
        [SerializeField, Min(0f)] private float bobAmplitude = 0.15f;
        [SerializeField, Min(0f)] private float bobFrequency = 0.2f;
        [SerializeField] private float phaseOffset;

        private Vector3 authoredLocalPosition;
        private Vector3 normalizedBobAxis = Vector3.up;

        public void Configure(Vector3 rotationSpeed, Vector3 axis, float amplitude, float frequency, float phase)
        {
            localRotationDegreesPerSecond = rotationSpeed;
            bobAxis = axis;
            bobAmplitude = Mathf.Max(0f, amplitude);
            bobFrequency = Mathf.Max(0f, frequency);
            phaseOffset = phase;
            CacheAuthoredState();
        }

        private void Awake()
        {
            CacheAuthoredState();
        }

        private void OnEnable()
        {
            CacheAuthoredState();
        }

        private void LateUpdate()
        {
            if (localRotationDegreesPerSecond.sqrMagnitude > 0f)
            {
                transform.Rotate(localRotationDegreesPerSecond * Time.deltaTime, Space.Self);
            }

            if (bobAmplitude <= 0f || bobFrequency <= 0f)
            {
                return;
            }

            float time = Time.time + phaseOffset;
            float bob = Mathf.Sin(time * bobFrequency * Mathf.PI * 2f) * bobAmplitude;
            transform.localPosition = authoredLocalPosition + normalizedBobAxis * bob;
        }

        private void CacheAuthoredState()
        {
            authoredLocalPosition = transform.localPosition;
            normalizedBobAxis = bobAxis.sqrMagnitude > 0.0001f ? bobAxis.normalized : Vector3.up;
        }
    }
}
