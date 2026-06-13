using UnityEngine;

namespace DimensionBrawl.Presentation
{
    [DisallowMultipleComponent]
    public sealed class ActionFoundationArenaFloatingShape : MonoBehaviour
    {
        [Header("Motion")]
        [SerializeField] private Vector3 localRotationDegreesPerSecond = new Vector3(12f, 28f, 8f);
        [SerializeField] private Vector3 bobAxis = Vector3.up;
        [SerializeField, Min(0f)] private float bobAmplitude = 0.25f;
        [SerializeField, Min(0f)] private float bobFrequency = 0.6f;
        [SerializeField] private float phaseOffset;

        [Header("Pulse")]
        [SerializeField] private Renderer[] renderers;
        [SerializeField] private Color baseColor = new Color(0.28f, 0.78f, 1f, 1f);
        [SerializeField] private Color emissionColor = new Color(0.12f, 0.85f, 1.4f, 1f);
        [SerializeField, Min(0f)] private float pulseAmplitude = 0.35f;
        [SerializeField, Min(0f)] private float pulseFrequency = 0.7f;

        private readonly int baseColorId = Shader.PropertyToID("_BaseColor");
        private readonly int colorId = Shader.PropertyToID("_Color");
        private readonly int emissionColorId = Shader.PropertyToID("_EmissionColor");

        private MaterialPropertyBlock propertyBlock;
        private Vector3 authoredLocalPosition;
        private Vector3 normalizedBobAxis = Vector3.up;

        public void Configure(
            Vector3 rotationSpeed,
            Vector3 axis,
            float amplitude,
            float bobHz,
            float phase,
            Color color,
            Color emission,
            float pulseAmount,
            float pulseHz)
        {
            localRotationDegreesPerSecond = rotationSpeed;
            bobAxis = axis;
            bobAmplitude = Mathf.Max(0f, amplitude);
            bobFrequency = Mathf.Max(0f, bobHz);
            phaseOffset = phase;
            baseColor = color;
            emissionColor = emission;
            pulseAmplitude = Mathf.Max(0f, pulseAmount);
            pulseFrequency = Mathf.Max(0f, pulseHz);
            CacheAuthoredState();
            ApplyPulse(0f);
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
            float time = Time.time + phaseOffset;
            if (localRotationDegreesPerSecond.sqrMagnitude > 0f)
            {
                transform.Rotate(localRotationDegreesPerSecond * Time.deltaTime, Space.Self);
            }

            if (bobAmplitude > 0f && bobFrequency > 0f)
            {
                float bob = Mathf.Sin(time * bobFrequency * Mathf.PI * 2f) * bobAmplitude;
                transform.localPosition = authoredLocalPosition + normalizedBobAxis * bob;
            }

            ApplyPulse(time);
        }

        private void CacheAuthoredState()
        {
            authoredLocalPosition = transform.localPosition;
            normalizedBobAxis = bobAxis.sqrMagnitude > 0.0001f ? bobAxis.normalized : Vector3.up;
            if (renderers == null || renderers.Length == 0)
            {
                renderers = GetComponentsInChildren<Renderer>(includeInactive: true);
            }

            propertyBlock ??= new MaterialPropertyBlock();
        }

        private void ApplyPulse(float time)
        {
            if (renderers == null)
            {
                return;
            }

            float pulse = pulseFrequency > 0f
                ? 1f + Mathf.Sin(time * pulseFrequency * Mathf.PI * 2f) * pulseAmplitude
                : 1f;
            Color pulsedEmission = emissionColor * Mathf.Max(0f, pulse);

            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer targetRenderer = renderers[i];
                if (targetRenderer == null)
                {
                    continue;
                }

                propertyBlock ??= new MaterialPropertyBlock();
                targetRenderer.GetPropertyBlock(propertyBlock);
                propertyBlock.SetColor(baseColorId, baseColor);
                propertyBlock.SetColor(colorId, baseColor);
                propertyBlock.SetColor(emissionColorId, pulsedEmission);
                targetRenderer.SetPropertyBlock(propertyBlock);
            }
        }
    }
}
